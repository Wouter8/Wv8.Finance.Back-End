namespace PersonalFinance.Business.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise;
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
        /// Initializes a new instance of the <see cref="SplitwiseManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public SplitwiseManager(Context context, ISplitwiseContext splitwiseContext)
            : base(context)
        {
            this.splitwiseContext = splitwiseContext;
        }

        /// <inheritdoc />
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool onlyImportable)
        {
            return this.Context.SplitwiseTransactions
                // Only transactions where nothing has been paid are importable.
                .WhereIf(onlyImportable, t => !t.Imported && t.PaidAmount == 0)
                .OrderBy(t => t.Date)
                .AsEnumerable()
                .Select(t => t.AsSplitwiseTransaction())
                .ToList();
        }

        /// <inheritdoc />
        public List<SplitwiseUser> GetSplitwiseUsers()
        {
            return this.splitwiseContext.GetUsers().Select(u => u.AsSplitwiseUser()).ToList();
        }

        /// <inheritdoc />
        public Transaction CompleteTransactionImport(int splitwiseId, int categoryId)
        {
            var splitwiseTransaction = this.Context.SplitwiseTransactions.GetEntity(splitwiseId);
            var splitwiseAccount = this.Context.Accounts.GetSplitwiseEntity();

            var category = this.Context.Categories.GetEntity(categoryId, allowObsolete: false);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                if (splitwiseTransaction.PaidAmount > 0)
                {
                    throw new ValidationException(
                            "A Splitwise transaction that has a paid share is managed in this application.");
                }

                var transaction = splitwiseTransaction.ToTransaction(splitwiseAccount, category);

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
            var newExpenses = this.splitwiseContext.GetExpenses(lastRan)
                // Only import expenses where the user did not pay, since these are not managed via the finance application.
                // Note that updates to expenses with a paid amount should be manually handled.
                .Where(e => e.PaidAmount == 0)
                .ToList();
            var newExpenseIds = newExpenses.Select(t => t.Id).ToSet();

            // Load relevant entities and store them in a dictionary.
            var splitwiseTransactionsById = this.Context.SplitwiseTransactions
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

                    var transaction = transactionsBySplitwiseId.TryGetValue(newExpense.Id);

                    // Revert the transaction before updating values.
                    if (transaction.IsSome)
                    {
                        processor.RevertIfProcessed(transaction.Value);

                        // Remove the transaction, it is re-added if needed.
                        this.Context.Transactions.Remove(transaction.Value);
                    }

                    var splitwiseTransaction = splitwiseTransactionMaybe
                        .Match(
                            sw => sw.UpdateValues(newExpense),
                            newExpense.ToSplitwiseTransactionEntity());

                    // If the transaction was already completely imported, then also update the transaction.
                    if (transaction.IsSome)
                    {
                        if (!splitwiseTransaction.IsDeleted)
                        {
                            // If the account or category is now obsolete, then the Splitwise transaction has to be re-imported.
                            if (transaction.Value.Account.IsObsolete || transaction.Value.Category.IsObsolete)
                            {
                                splitwiseTransaction.Imported = false;
                            }
                            // Otherwise, create a new transaction for the new Splitwise transaction.
                            else
                            {
                                transaction =
                                    splitwiseTransaction.ToTransaction(transaction.Value.Account,
                                        transaction.Value.Category);

                                processor.ProcessIfNeeded(transaction.Value);

                                this.Context.Transactions.Add(transaction.Value);
                            }
                        }
                    }

                    // If it is a new expense, then add it to the context.
                    if (splitwiseTransactionMaybe.IsNone)
                        this.Context.SplitwiseTransactions.Add(splitwiseTransaction);

                    // If the transaction existed and has not yet been imported, then the transaction is already updated above.
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
                LastRunTimestamp = lastRunTimestamp,
                CurrentState = SplitwiseManager.importStatus,
            };
        }
    }
}