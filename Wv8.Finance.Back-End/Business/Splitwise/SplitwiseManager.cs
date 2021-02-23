namespace PersonalFinance.Business.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.Models;
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
        public Transaction ImportTransaction(int splitwiseId, int categoryId)
        {
            var splitwiseTransaction = this.Context.SplitwiseTransactions.GetEntity(splitwiseId);
            var splitwiseAccount = this.Context.Accounts.GetSplitwiseEntity();

            var category = this.Context.Categories.GetEntity(categoryId, allowObsolete: false);

            return this.ConcurrentInvoke(() =>
            {
                // If the user paid anything for the transaction, then create an expense transaction for the default account.
                if (splitwiseTransaction.PaidAmount > 0)
                {
                    throw new ValidationException(
                            "A Splitwise transaction that has a paid share is managed in this application.");
                }

                var transaction = splitwiseTransaction.ToTransaction(splitwiseAccount, category);

                transaction.ProcessIfNeeded(this.Context);

                this.Context.Transactions.Add(transaction);

                this.Context.SaveChanges();

                return transaction.AsTransaction();
            });
        }

        /// <inheritdoc />
        public void ImportFromSplitwise()
        {
            // We request all expenses from Splitwise which were updated after the last known updated at timestamp.
            // This way we always get all new expenses.
            var latestUpdated = this.Context.SplitwiseTransactions
                .Select(t => t.UpdatedAt)
                .OrderByDescending(t => t)
                .Take(1)
                .AsEnumerable()
                .SingleOrNone()
                .ValueOrElse(DateTime.MinValue);

            // Get the new and updated expenses from Splitwise.
            var newExpenses = this.splitwiseContext.GetExpenses(latestUpdated)
                // Filter expenses for which the user paid, as these are managed from this application.
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
                foreach (var newExpense in newExpenses)
                {
                    var splitwiseTransactionMaybe = splitwiseTransactionsById.TryGetValue(newExpense.Id);
                    var knownSplitwiseTransaction = splitwiseTransactionMaybe.IsSome;
                    var splitwiseTransaction = splitwiseTransactionMaybe.ValueOrElse(new SplitwiseTransactionEntity());
                    var transaction = transactionsBySplitwiseId.TryGetValue(splitwiseTransaction.Id);

                    // Revert the transaction before updating values.
                    if (transaction.IsSome)
                    {
                        transaction.Value.RevertIfProcessed(this.Context);

                        // Remove the transaction, it is re-added if needed.
                        this.Context.Transactions.Remove(transaction.Value);
                    }

                    // Set all fields on new or known expense.
                    splitwiseTransaction.Id = newExpense.Id;
                    splitwiseTransaction.Date = newExpense.Date;
                    splitwiseTransaction.Description = newExpense.Description;
                    splitwiseTransaction.IsDeleted = newExpense.IsDeleted;
                    splitwiseTransaction.UpdatedAt = newExpense.UpdatedAt;
                    splitwiseTransaction.PaidAmount = newExpense.PaidAmount;
                    splitwiseTransaction.PersonalAmount = newExpense.PersonalAmount;

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
                                    splitwiseTransaction.ToTransaction(transaction.Value.Account, transaction.Value.Category);

                                transaction.Value.ProcessIfNeeded(this.Context);

                                this.Context.Transactions.Add(transaction.Value);
                            }
                        }
                    }

                    // If it is a new expense, then add it to the context.
                    if (!knownSplitwiseTransaction)
                        this.Context.SplitwiseTransactions.Add(splitwiseTransaction);

                    // If the transaction existed and has not yet been imported, then the transaction is already updated above.
                }

                this.Context.SaveChanges();
            });
        }
    }
}