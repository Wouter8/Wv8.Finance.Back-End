namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Comparers;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class TransactionConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static Transaction AsTransaction(this TransactionEntity entity)
        {
            if (entity.CategoryId.HasValue && entity.Category == null)
                throw new ArgumentNullException(nameof(entity.Category));
            if (entity.Account == null)
                throw new ArgumentNullException(nameof(entity.Account));
            if (entity.ReceivingAccountId.HasValue && entity.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(entity.ReceivingAccount));

            return new Transaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Amount = entity.Amount,
                Date = entity.Date.ToDateString(),
                Type = entity.Type,
                CategoryId = entity.CategoryId.ToMaybe(),
                Category = entity.Category.ToMaybe().Select(c => c.AsCategory()),
                AccountId = entity.AccountId,
                Account = entity.Account.AsAccount(),
                ReceivingAccountId = entity.ReceivingAccountId.ToMaybe(),
                ReceivingAccount = entity.ReceivingAccount.ToMaybe().Select(a => a.AsAccount()),
                Processed = entity.Processed,
                RecurringTransactionId = entity.RecurringTransactionId.ToMaybe(),
                RecurringTransaction = entity.RecurringTransaction.ToMaybe().Select(t => t.AsRecurringTransaction()),
                NeedsConfirmation = entity.NeedsConfirmation,
                IsConfirmed = entity.IsConfirmed.ToMaybe(),
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entities">The entity.</param>
        /// <param name="totalSearchResults">The total search results retrieved. Might not be the same as the
        /// length of the list because of pagination parameters.</param>
        /// <returns>The data transfer object.</returns>
        public static TransactionGroup AsTransactionGroup(this List<TransactionEntity> entities, int totalSearchResults)
        {
            var transactions = entities.Select(e => e.AsTransaction()).ToList();
            var transactionsWithCategory = transactions.Where(t => t.CategoryId.IsSome).ToList();

            var categories = transactionsWithCategory
                .Select(t => t.Category.Value)
                .Distinct(new CategoryComparer())
                .ToDictionary(c => c.Id);

            return new TransactionGroup
            {
                TotalSearchResults = totalSearchResults,
                TotalSum = transactions.Sum(t => t.Amount),
                Transactions = transactions,
                // Not using .ToLookup() as this will need a special JSON-converter.
                SumPerExpenseCategory = transactionsWithCategory
                    .Where(t => t.Category.Value.Type == CategoryType.Expense)
                    .GroupBy(t => t.Category.Value.Id, t => t)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                SumPerIncomeCategory = transactionsWithCategory
                    .Where(t => t.Category.Value.Type == CategoryType.Income)
                    .GroupBy(t => t.Category.Value.Id, t => t)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                TransactionsPerCategory = transactionsWithCategory
                    .GroupBy(t => t.Category.Value.Id, t => t)
                    .ToDictionary(g => g.Key, g => g.ToList()),
                TransactionsPerType = transactions
                    .GroupBy(t => t.Type)
                    .ToDictionary(g => g.Key, g => g.ToList()),
                Categories = categories,
            };
        }
    }
}