namespace PersonalFinance.Business.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Options;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// A manager for functionality related to Splitwise.
    /// </summary>
    public class SplitwiseManager : BaseManager, ISplitwiseManager
    {
        /// <summary>
        /// The lock object, to make sure importing is not done multiple times.
        /// </summary>
        private static readonly object lockObj = new ();

        /// <summary>
        /// The current status of the importer.
        /// </summary>
        private static ImportState importStatus = ImportState.NotRunning;

        /// <summary>
        /// The Splitwise context.
        /// </summary>
        private readonly ISplitwiseContext splitwiseContext;

        /// <summary>
        /// The user identifier in Splitwise.
        /// </summary>
        private int splitwiseUserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitwiseManager"/> class.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <param name="context">The database context.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public SplitwiseManager(IOptions<ApplicationSettings> settings, Context context, ISplitwiseContext splitwiseContext)
            : base(context)
        {
            this.splitwiseContext = splitwiseContext;
            this.splitwiseUserId = settings.Value.SplitwiseUserId;
        }

        /// <inheritdoc />
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool onlyImportable)
        {
            return this.Context.SplitwiseTransactions
                .WhereIf(onlyImportable, t => !t.IsDeleted && !t.Imported)
                .OrderBy(t => t.Date)
                .AsEnumerable()
                .Select(t => t.AsSplitwiseTransaction())
                .ToList();
        }

        /// <inheritdoc />
        public List<SplitwiseUser> GetSplitwiseUsers()
        {
            return this.splitwiseContext.GetUsers()
                .Select(u => u.AsSplitwiseUser())
                .OrderBy(u => u.Name)
                .ToList();
        }

        /// <inheritdoc />
        public Transaction CompleteTransactionImport(int splitwiseId, int categoryId, Maybe<int> accountId)
        {
            var splitwiseTransaction = this.Context.SplitwiseTransactions.GetEntity(splitwiseId);
            var splitwiseAccount = this.Context.Accounts.GetSplitwiseEntity();

            var category = this.Context.Categories.GetEntity(categoryId, allowObsolete: false);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                if (splitwiseTransaction.PaidAmount > 0 && accountId.IsNone)
                {
                    throw new ValidationException(
                            "An account should be specified for a Splitwise transaction that has a paid share.");
                }

                var account = splitwiseTransaction.PaidAmount > 0
                    ? this.Context.Accounts.GetEntity(accountId.Value)
                    : splitwiseAccount;

                var transaction = splitwiseTransaction.ToTransaction(account, category);

                processor.ProcessIfNeeded(transaction);

                this.Context.Transactions.Add(transaction);

                this.Context.SaveChanges();

                return transaction.AsTransaction();
            });
        }

        /// <inheritdoc />
        public ImportResult ImportFromSplitwise()
        {
            lock (SplitwiseManager.lockObj)
            {
                if (SplitwiseManager.importStatus == ImportState.Running)
                    return ImportResult.AlreadyRunning;

                SplitwiseManager.importStatus = ImportState.Running;
            }

            var lastRan = this.Context.SynchronizationTimes
                .Single()
                .SplitwiseLastRun;

            // Get the new and updated expenses from Splitwise.
            var timestamp = DateTime.UtcNow;
            var newExpenses = this.splitwiseContext.GetExpenses(lastRan);
            var newExpenseIds = newExpenses.Select(t => t.Id).ToSet();

            // Load relevant entities and store them in a dictionary.
            var splitwiseTransactionsById = this.Context.SplitwiseTransactions
                .IncludeAll()
                .Where(t => newExpenseIds.Contains(t.Id))
                .AsEnumerable()
                .ToDictionary(t => t.Id);
            var transactionsBySplitwiseId = this.Context.Transactions
                .IncludeAll()
                .Where(t => t.SplitwiseTransactionId.HasValue)
                .Where(t => newExpenseIds.Contains(t.SplitwiseTransactionId.Value))
                .AsEnumerable()
                .ToDictionary(t => t.SplitwiseTransactionId.Value);

            this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                this.Context.SetSplitwiseSynchronizationTime(timestamp);

                foreach (var newExpense in newExpenses)
                {
                    var splitwiseTransactionMaybe = splitwiseTransactionsById.TryGetValue(newExpense.Id);

                    if (splitwiseTransactionMaybe.IsSome &&
                        splitwiseTransactionMaybe.Value.UpdatedAt == newExpense.UpdatedAt)
                    {
                        // The last updated at is equal to the one stored, meaning that the latest update was
                        // triggered by this application and is already handled.
                        continue;
                    }

                    // The new expense is not known and the user has no share, so it's irrelevant.
                    if (splitwiseTransactionMaybe.IsNone && !newExpense.HasShare)
                        continue;

                    var transaction = transactionsBySplitwiseId.TryGetValue(newExpense.Id);

                    // Revert the transaction before updating values.
                    if (transaction.IsSome)
                    {
                        processor.RevertIfProcessed(transaction.Value);

                        // Remove the transaction, it is re-added if needed.
                        this.Context.Transactions.Remove(transaction.Value);
                    }

                    // Update the values of the Splitwise transaction, or create a new one.
                    var splitwiseTransaction = splitwiseTransactionMaybe.Match(
                        st =>
                        {
                            this.Context.SplitDetails.RemoveRange(st.SplitDetails);
                            st.UpdateValues(newExpense);
                            st.SplitDetails = newExpense.Splits.Select(s => s.ToSplitDetailEntity()).ToList();

                            return st;
                        },
                        () =>
                        {
                            var st = newExpense.ToSplitwiseTransactionEntity();
                            this.Context.SplitwiseTransactions.Add(st);
                            return st;
                        });

                    // Remove the Splitwise transaction if it is irrelevant
                    if (!splitwiseTransaction.HasShare)
                    {
                        this.Context.SplitwiseTransactions.Remove(splitwiseTransaction);
                        continue;
                    }

                    // If the Splitwise transaction was already completely imported and is importable after the update,
                    // then try to update the transaction.
                    if (transaction.IsSome && splitwiseTransaction.Importable &&
                        // If the account or category is now obsolete, then the Splitwise transaction has to be re-imported.
                        !transaction.Value.Account.IsObsolete && !transaction.Value.Category.IsObsolete)
                    {
                        transaction = splitwiseTransaction.ToTransaction(
                            transaction.Value.Account, transaction.Value.Category);

                        this.Context.Transactions.Add(transaction.Value);

                        processor.ProcessIfNeeded(transaction.Value);
                    }
                }

                this.Context.SaveChanges();
            });

            SplitwiseManager.importStatus = ImportState.NotRunning;

            return ImportResult.Completed;
        }

        /// <inheritdoc />
        public ImporterInformation GetImporterInformation()
        {
            var lastRunTimestamp = this.Context.SynchronizationTimes.Single().SplitwiseLastRun;

            return new ImporterInformation
            {
                LastRunTimestamp = lastRunTimestamp.ToDateTimeString(),
                CurrentState = SplitwiseManager.importStatus,
            };
        }
    }
}