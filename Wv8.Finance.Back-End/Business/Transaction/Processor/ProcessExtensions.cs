namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class providing extension methods to process transactions.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Processes a transaction. Meaning the value is added to the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="context">The database context.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        /// <returns>The updated transaction.</returns>
        public static TransactionEntity ProcessTransaction(this TransactionEntity transaction, Context context)
        {
            transaction.VerifyEntitiesNotObsolete();

            var historicalEntriesToEdit = GetBalanceEntriesToEdit(transaction.Account, transaction.Date);

            switch (transaction.Type)
            {
                case TransactionType.External:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    // Update budgets.
                    if (transaction.Amount < 0)
                    {
                        var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                        foreach (var budget in budgets)
                            budget.Spent += Math.Abs(transaction.Amount);
                    }

                    break;
                case TransactionType.Internal:
                    var receiverEntriesToEdit =
                        GetBalanceEntriesToEdit(transaction.ReceivingAccount, transaction.Date);

                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance -= transaction.Amount;
                    foreach (var entry in receiverEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    break;
                default:
                    throw new InvalidOperationException("Unknown transaction type.");
            }

            transaction.Processed = true;

            return transaction;
        }

        /// <summary>
        /// Reverses the processing of a transaction. Meaning the value is removed from the account, budgets and savings.
        /// This alters all historical values starting at the transaction date.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="context">The database context.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        /// <returns>The updated transaction.</returns>
        public static TransactionEntity RevertProcessedTransaction(this TransactionEntity transaction, Context context)
        {
            if (!transaction.Processed)
                throw new NotSupportedException("Transaction has not been processed.");

            var historicalBalances = transaction.Account.DailyBalances
                .Where(hb => hb.Date >= transaction.Date)
                .ToList();

            switch (transaction.Type)
            {
                case TransactionType.External:
                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance -= transaction.Amount;

                    // Update budgets.
                    if (transaction.Amount < 0)
                    {
                        var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                        foreach (var budget in budgets)
                            budget.Spent -= Math.Abs(transaction.Amount);
                    }

                    break;
                case TransactionType.Internal:
                    var receiverHistoricalBalances = transaction.ReceivingAccount.DailyBalances
                        .Where(hb => hb.Date >= transaction.Date)
                        .ToList();

                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance += transaction.Amount;
                    foreach (var historicalBalance in receiverHistoricalBalances)
                        historicalBalance.Balance -= transaction.Amount;

                    break;
            }

            transaction.Processed = false;

            return transaction;
        }

        /// <summary>
        /// Processes a recurring transaction. Meaning that instances get created.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <param name="context">The database context.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        public static void ProcessRecurringTransaction(
            this RecurringTransactionEntity transaction,
            Context context)
        {
            transaction.VerifyEntitiesNotObsolete();

            var instances = new List<TransactionEntity>();
            while (!transaction.Finished)
            {
                // No more transactions need to be created.
                if (transaction.NextOccurence > LocalDate.FromDateTime(DateTime.Today))
                    break;

                var instance = transaction.CreateOccurence();

                // Immediately process if transaction does not need to be confirmed.
                if (!instance.NeedsConfirmation)
                    instance.ProcessTransaction(context);

                instances.Add(instance);
            }

            context.Transactions.AddRange(instances);
        }

        /// <summary>
        /// Calculates and sets the next occurrence for a recurring transaction.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        private static void SetNextOccurrence(this RecurringTransactionEntity transaction)
        {
            var start = transaction.LastOccurence ?? transaction.StartDate;
            var next = LocalDate.MinIsoValue;
            switch (transaction.IntervalUnit)
            {
                case IntervalUnit.Days:
                    next = start.PlusDays(transaction.Interval);
                    break;
                case IntervalUnit.Weeks:
                    next = start.PlusWeeks(transaction.Interval);
                    break;
                case IntervalUnit.Months:
                    next = start.PlusMonths(transaction.Interval);
                    break;
                case IntervalUnit.Years:
                    next = start.PlusYears(transaction.Interval);
                    break;
            }

            if (!transaction.EndDate.HasValue || next <= transaction.EndDate)
            {
                transaction.NextOccurence = next;
            }
            else
            {
                transaction.NextOccurence = null;
                transaction.Finished = true;
            }
        }

        /// <summary>
        /// Creates a new occurence and calculates the next occurence for a recurring transaction.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <returns>The created transaction.</returns>
        /// <remarks>Note that the date is not validated in this method.</remarks>
        private static TransactionEntity CreateOccurence(this RecurringTransactionEntity transaction)
        {
            if (!transaction.NextOccurence.HasValue)
                throw new InvalidOperationException("Recurring transaction has no next occurence date set.");

            var instance = new TransactionEntity
            {
                AccountId = transaction.AccountId,
                Account = transaction.Account,
                Amount = transaction.Amount,
                CategoryId = transaction.CategoryId,
                Category = transaction.Category,
                Date = transaction.NextOccurence.Value,
                Description = transaction.Description,
                Processed = false,
                ReceivingAccountId = transaction.ReceivingAccountId,
                ReceivingAccount = transaction.ReceivingAccount,
                RecurringTransactionId = transaction.Id,
                RecurringTransaction = transaction,
                NeedsConfirmation = transaction.NeedsConfirmation,
                IsConfirmed = transaction.NeedsConfirmation ? false : (bool?)null,
                Type = transaction.Type,
            };
            transaction.LastOccurence = transaction.NextOccurence.Value;

            transaction.SetNextOccurrence();

            return instance;
        }

        /// <summary>
        /// Gets the historical balance entries which should be edited based on a date. If the date has no entry,
        /// a new historical entry will be created and inserted in history.
        /// </summary>
        /// <param name="account">The account for which to check the historical entries.</param>
        /// <param name="date">The date from which should be checked.</param>
        /// <returns>The list of to be updated entities.</returns>
        private static List<DailyBalanceEntity> GetBalanceEntriesToEdit(AccountEntity account, LocalDate date)
        {
            var balanceEntriesAfterDate = account.DailyBalances
                .Where(hb => hb.Date >= date)
                .OrderBy(hb => hb.Date)
                .ToList();

            // If date already has historical entry, just alter that and later entries.
            var balanceEntryOnSameDate = account.DailyBalances
                .SingleOrNone(hb => hb.Date == date);
            if (balanceEntryOnSameDate.IsSome)
            {
                return balanceEntriesAfterDate;
            }

            // Get last entity before date
            var lastEntry = account.DailyBalances
                .Where(hb => hb.Date < date)
                .OrderBy(hb => hb.Date)
                .LastOrNone();

            var newBalanceEntry = new DailyBalanceEntity
            {
                Balance = lastEntry.Select(e => e.Balance).ValueOrElse(0),
                Date = date,
            };
            account.DailyBalances.Add(newBalanceEntry);

            // Return all entries after the date + the new entry
            return newBalanceEntry.Enumerate()
                    .Concat(balanceEntriesAfterDate)
                    .ToList();
        }

        /// <summary>
        /// Verifies that the entities linked to a transaction are not obsolete.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private static void VerifyEntitiesNotObsolete(this TransactionEntity transaction)
        {
            if (transaction.Account == null)
                throw new ArgumentNullException(nameof(transaction.Account));
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(transaction.ReceivingAccount));
            if (transaction.CategoryId.HasValue && transaction.Category == null)
                throw new ArgumentNullException(nameof(transaction.Category));

            if (transaction.Account.IsObsolete)
                throw new IsObsoleteException($"Account is obsolete.");
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount.IsObsolete)
                throw new IsObsoleteException($"Receiver is obsolete.");
            if (transaction.CategoryId.HasValue && transaction.Category.IsObsolete)
                throw new IsObsoleteException($"Category is obsolete.");
        }

        /// <summary>
        /// Verifies that the entities linked to a recurring transaction are not obsolete.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        private static void VerifyEntitiesNotObsolete(this RecurringTransactionEntity transaction)
        {
            if (transaction.Account == null)
                throw new ArgumentNullException(nameof(transaction.Account));
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(transaction.ReceivingAccount));
            if (transaction.CategoryId.HasValue && transaction.Category == null)
                throw new ArgumentNullException(nameof(transaction.Category));

            if (transaction.Account.IsObsolete)
                throw new IsObsoleteException($"Account is obsolete.");
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount.IsObsolete)
                throw new IsObsoleteException($"Receiver is obsolete.");
            if (transaction.CategoryId.HasValue && transaction.Category.IsObsolete)
                throw new IsObsoleteException($"Category is obsolete.");
        }
    }
}