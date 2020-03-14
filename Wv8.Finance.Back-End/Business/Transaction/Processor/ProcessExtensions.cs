namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.History;
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

            var (processedAt, historicalEntriesToEdit) = GetHistoricalEntriesToEdit(transaction.Account, transaction.Date, context);

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance -= Math.Abs(transaction.Amount);

                    // Update budgets.
                    var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                        budget.Spent += Math.Abs(transaction.Amount);

                    break;
                case TransactionType.Income:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    break;
                case TransactionType.Transfer:
                    var (_, receiverEntriesToEdit) =
                        GetHistoricalEntriesToEdit(transaction.ReceivingAccount, transaction.Date, context);

                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance -= transaction.Amount;
                    foreach (var entry in receiverEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    break;
            }

            transaction.ProcessedAt = processedAt;
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

            var accountHistory = transaction.Account.History
                .Between(transaction.ProcessedAt.Value, DateTime.MaxValue)
                .ToList();

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var accountHistoryEntry in accountHistory)
                        accountHistoryEntry.Balance += Math.Abs(transaction.Amount);

                    // Update budgets.
                    var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);

                    foreach (var budget in budgets)
                        budget.Spent -= Math.Abs(transaction.Amount);

                    break;
                case TransactionType.Income:
                    foreach (var accountHistoryEntry in accountHistory)
                        accountHistoryEntry.Balance -= transaction.Amount;

                    break;
                case TransactionType.Transfer:
                    var receiverHistory = transaction.ReceivingAccount.History
                        .Between(transaction.ProcessedAt.Value, DateTime.MaxValue)
                        .ToList();

                    foreach (var accountHistoryEntry in accountHistory)
                        accountHistoryEntry.Balance += transaction.Amount;
                    foreach (var accountHistoryEntry in receiverHistory)
                        accountHistoryEntry.Balance -= transaction.Amount;

                    break;
            }

            transaction.Processed = false;
            transaction.ProcessedAt = null;

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
                if (transaction.NextOccurence > DateTime.Today)
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
            var next = DateTime.MinValue;
            switch (transaction.IntervalUnit)
            {
                case IntervalUnit.Days:
                    next = start.AddDays(transaction.Interval);
                    break;
                case IntervalUnit.Weeks:
                    next = start.AddDays(7 * transaction.Interval);
                    break;
                case IntervalUnit.Months:
                    next = start.AddMonths(transaction.Interval);
                    break;
                case IntervalUnit.Years:
                    next = start.AddYears(transaction.Interval);
                    break;
            }

            if (next <= transaction.EndDate)
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
        /// Gets the historical entries which should be edited based on a date. If the date has no entry,
        /// a new historical entry will be created and inserted in history.
        /// </summary>
        /// <param name="account">The account for which to check the historical entries.</param>
        /// <param name="date">The date from which should be checked.</param>
        /// <param name="context">The database context.</param>
        /// <returns>The list of to be updated entities and the date and the first datetime on the specified date.</returns>
        private static (DateTime, List<AccountHistoryEntity>) GetHistoricalEntriesToEdit(AccountEntity account, DateTime date, Context context)
        {
            var currentHistoryEntry = account.History.SingleAtNow();

            // If transaction has same or later date as latest account update, then we can just add another one.
            // If transaction is in the past, we have to alter the historical entries.
            var latestDate = currentHistoryEntry.ValidFrom.Date <= date;
            var historyItems = latestDate
                ? new List<AccountHistoryEntity> { currentHistoryEntry.NewHistoricalEntry(context) }
                : account.History
                    .Between(date, DateTime.MaxValue)
                    .OrderBy(h => h.ValidFrom)
                    .ToList();

            var processedAt = context.CreationTime;
            if (!latestDate)
            {
                // If the first entry has the same start date as the wanted date, just use that one.
                if (historyItems[0].ValidFrom.Date == date)
                {
                    processedAt = historyItems[0].ValidFrom;
                }
                // It can be that the first historical entry starts later than the provided date. Then we insert an entity before it.
                else if (historyItems[0].ValidFrom.Date > date)
                {
                    var firstEntry = CreateHistoryEntityBefore(historyItems[0], date, context);
                    historyItems.Insert(0, firstEntry);

                    processedAt = firstEntry.ValidFrom;
                }
                // The first entry must contain the end of the previous day, so the second entry might be an update on the date of the to be processed transaction.
                // If it is not the date of the transaction, we have to add a historical entry in between the first and second historical entries in the list.
                else
                {
                    var first = historyItems[0];
                    var second = historyItems[1];
                    if (second.ValidFrom.Date != date)
                    {
                        var created =
                            CreateHistoryEntityInBetween(first, second, date, context);

                        // Insert at second index because the first is always the update at 00:00, which is the end balance of the previous day.
                        // We are inserting a balance change one the following day.
                        historyItems.Insert(1, created);

                        processedAt = created.ValidFrom;
                    }
                    else
                    {
                        processedAt = second.ValidFrom;
                    }
                }
            }

            return (processedAt, historyItems);
        }

        /// <summary>
        /// Creates a historical entry before an already existing entry.
        /// This is needed because a transaction can be added/processed with a date in the past,
        /// while the first historical entry is later than this date.
        /// </summary>
        /// <typeparam name="T">The type of the historical entity.</typeparam>
        /// <param name="a">The historical entry that will be the next entry.</param>
        /// <param name="date">The date on which the entry has to be created.</param>
        /// <param name="context">The database context.</param>
        /// <returns>The created historical entity.</returns>
        private static T CreateHistoryEntityBefore<T>(T a, DateTime date, Context context)
            where T : class, IHistoricalEntity
        {
            if (a.ValidFrom.Date == date.Date)
            {
                // Because this method is pretty dangerous, only allow it if it adds an entry for a new day.
                throw new InvalidOperationException(
                    "Date is the same as the update, just update the entry on the same date.");
            }

            // New datetime after 00:00 to prevent altering the end balance of the previous day.
            var startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 1, DateTimeKind.Utc);
            var endDate = a.ValidFrom;

            // Clone and set valid to to be the start of the existing entry.
            var newEntity = (T)a.Clone();
            newEntity.ValidFrom = startDate;
            newEntity.ValidTo = endDate;

            context.Entry(newEntity).State = EntityState.Added;

            return newEntity;
        }

        /// <summary>
        /// Creates a historical entry in between already existing historical entries.
        /// This is needed because a transaction can be added/processed with a date in the past.
        /// </summary>
        /// <typeparam name="T">The type of the historical entity.</typeparam>
        /// <param name="a">The historical entry that will be the previous entry.</param>
        /// <param name="b">The historical entry that will be the following entry.</param>
        /// <param name="date">The date on which the entry has to be created.</param>
        /// <param name="context">The database context.</param>
        /// <returns>The created historical entity.</returns>
        private static T CreateHistoryEntityInBetween<T>(T a, T b, DateTime date, Context context)
            where T : class, IHistoricalEntity
        {
            if (a.ValidTo != b.ValidFrom)
            {
                throw new InvalidOperationException(
                    "Can not create a historical entry between entries that are not next to each other.");
            }

            if (b.ValidFrom.Date == date.Date)
            {
                // Because this method is pretty dangerous, only allow it if it adds an entry for a new day.
                throw new InvalidOperationException(
                    "Date is the same as the second update, just update the entry on the same date.");
            }

            // New datetime after 00:00 to prevent altering the end balance of the previous day.
            var startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 1, DateTimeKind.Utc);
            var endDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 2, DateTimeKind.Utc);

            a.ValidTo = startDate;
            b.ValidFrom = endDate;

            // Clone the first, since we build upon that.
            var newEntity = (T)a.Clone();
            newEntity.ValidFrom = startDate;
            newEntity.ValidTo = endDate;

            context.Entry(newEntity).State = EntityState.Added;

            return newEntity;
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