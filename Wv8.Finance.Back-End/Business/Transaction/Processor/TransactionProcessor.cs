namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class which handles transactions and recurring objects.
    /// </summary>
    public class TransactionProcessor : BaseManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionProcessor"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TransactionProcessor(Context context)
            : base(context)
        {
        }

        /// <summary>
        /// Processes all entities that need to be processed.
        /// </summary>
        public void ProcessAll()
        {
            this.ConcurrentInvoke(() =>
            {
                this.ProcessTransactions();
                this.ProcessRecurringBudgets();
                this.ProcessRecurringTransactions();

                this.Context.SaveChanges();
            });
        }

        #region Checks

        /// <summary>
        /// Processes a specific transaction if the transaction needs to be processed.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void ProcessIfNeeded(TransactionEntity transaction)
        {
            if (transaction.NeedsProcessing())
                this.Process(transaction);
        }

        /// <summary>
        /// Reverts the processing of a transaction if it was already processed.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void RevertIfProcessed(TransactionEntity transaction)
        {
            if (transaction.Processed)
                this.Revert(transaction);
        }

        /// <summary>
        /// Processes a specific recurring transaction if the recurring transaction has a start date which is before or
        /// equal to the current date.
        /// </summary>
        /// <param name="recurringTransaction">The recurring transaction.</param>
        public void ProcessIfNeeded(RecurringTransactionEntity recurringTransaction)
        {
            if (recurringTransaction.NeedsProcessing())
                this.Process(recurringTransaction);
        }

        #endregion Checks

        #region Single

        /// <summary>
        /// Processes a transaction. Meaning the value is added to the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(TransactionEntity transaction)
        {
            transaction.VerifyEntitiesNotObsolete();

            var historicalEntriesToEdit = this.GetBalanceEntriesToEdit(transaction.AccountId, transaction.Date);
            var amount = transaction.GetPersonalAmount();

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += amount;

                    // Update budgets.
                    var budgets = this.Context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                        budget.Spent += Math.Abs(amount);

                    break;
                case TransactionType.Income:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += amount;

                    break;
                case TransactionType.Transfer:
                    var receiverEntriesToEdit =
                        this.GetBalanceEntriesToEdit(transaction.ReceivingAccountId.Value, transaction.Date);

                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance -= amount;
                    foreach (var entry in receiverEntriesToEdit)
                        entry.Balance += amount;

                    break;
                default:
                    throw new InvalidOperationException("Unknown transaction type.");
            }

            transaction.Processed = true;
        }

        /// <summary>
        /// Reverses the processing of a transaction. Meaning the value is removed from the account, budgets and savings.
        /// This alters all historical values starting at the transaction date.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Revert(TransactionEntity transaction)
        {
            if (!transaction.Processed)
                throw new NotSupportedException("Transaction has not been processed.");

            var historicalBalances = this.GetBalanceEntriesToEdit(transaction.AccountId, transaction.Date);
            var amount = transaction.GetPersonalAmount();

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance -= amount;

                    // Update budgets.
                    var budgets = this.Context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                        budget.Spent -= Math.Abs(amount);

                    break;
                case TransactionType.Income:
                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance -= amount;

                    break;
                case TransactionType.Transfer:
                    var receiverHistoricalBalances = this.Context.DailyBalances
                        .Where(db => db.AccountId == transaction.ReceivingAccountId)
                        .Where(hb => hb.Date >= transaction.Date)
                        .ToList();

                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance += amount;
                    foreach (var historicalBalance in receiverHistoricalBalances)
                        historicalBalance.Balance -= amount;

                    break;
            }

            transaction.Processed = false;
        }

        /// <summary>
        /// Processes a recurring transaction. Meaning that instances get created.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(RecurringTransactionEntity transaction)
        {
            transaction.VerifyEntitiesNotObsolete();

            var instances = new List<TransactionEntity>();
            while (!transaction.Finished)
            {
                // Create transactions until a couple days in the future.
                if (transaction.NextOccurence > DateTime.Today.AddDays(7).ToLocalDate())
                    break;

                var instance = transaction.CreateOccurence();
                var isFuture = instance.Date > DateTime.Today.ToLocalDate();

                // Immediately process if transaction does not need to be confirmed.
                if (!instance.NeedsConfirmation && !isFuture)
                    this.Process(instance);

                instances.Add(instance);
            }

            this.Context.Transactions.AddRange(instances);
        }

        #endregion Single

        #region Bulk

        /// <summary>
        /// Processes the transactions that are in the past.
        /// </summary>
        private void ProcessTransactions()
        {
            var today = LocalDate.FromDateTime(DateTime.Today);
            var transactionsToBeProcessed = this.Context.Transactions
                .IncludeAll()
                .Where(t => !t.Processed && t.Date <= today &&
                            (!t.NeedsConfirmation || (t.NeedsConfirmation && t.IsConfirmed.Value)))
                .ToList();

            foreach (var transaction in transactionsToBeProcessed)
            {
                this.Process(transaction);
            }
        }

        /// <summary>
        /// Processes the recurring transactions that have to be created.
        /// </summary>
        private void ProcessRecurringTransactions()
        {
            var today = LocalDate.FromDateTime(DateTime.Today);
            var recurringTransactions = this.Context.RecurringTransactions
                .IncludeAll()
                .Where(rt => !rt.Finished && rt.StartDate <= today)
                .ToList();

            foreach (var recurringTransaction in recurringTransactions)
            {
                this.Process(recurringTransaction);
            }
        }

        /// <summary>
        /// Processes the recurring budgets that have to be created.
        /// </summary>
        private void ProcessRecurringBudgets()
        {
            // TODO: Implement
        }

        #endregion Bulk

        #region Helpers

        /// <summary>
        /// Gets the historical balance entries which should be edited based on a date. If the date has no entry,
        /// a new historical entry will be created and inserted in history.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="accountId">The account identifier for which to check the historical entries.</param>
        /// <param name="date">The date from which should be checked.</param>
        /// <returns>The list of to be updated entities.</returns>
        private List<DailyBalanceEntity> GetBalanceEntriesToEdit(int accountId, LocalDate date)
        {
            var balanceEntries = this.Context.DailyBalances
                .Where(db => db.AccountId == accountId)
                .ToList();
            var balanceEntriesAfterDate = balanceEntries
                .Where(hb => hb.Date >= date)
                .OrderBy(hb => hb.Date)
                .ToList();

            // If date already has historical entry, just alter that and later entries.
            var balanceEntryOnSameDate = balanceEntries.SingleOrNone(hb => hb.Date == date);
            if (balanceEntryOnSameDate.IsSome)
                return balanceEntriesAfterDate;

            // Get last entity before date
            var lastEntry = balanceEntries
                .Where(hb => hb.Date < date)
                .OrderBy(hb => hb.Date)
                .LastOrNone();

            var newBalanceEntry = new DailyBalanceEntity
            {
                AccountId = accountId,
                Balance = lastEntry.Select(e => e.Balance).ValueOrElse(0),
                Date = date,
            };
            this.Context.DailyBalances.Add(newBalanceEntry);

            // Return all entries after the date + the new entry
            return newBalanceEntry.Enumerate()
                    .Concat(balanceEntriesAfterDate)
                    .ToList();
        }

        #endregion Helpers
    }
}