namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common.Comparers;
    using PersonalFinance.Common.DataTransfer;
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

            return new Transaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Amount = entity.Amount,
                Date = entity.Date.ToString("O"),
                Type = entity.Type,
                CategoryId = entity.CategoryId.ToMaybe(),
                Category = entity.Category.ToMaybe().Select(c => c.AsCategory()),
                Settled = entity.Settled,
                RecurringTransactionId = entity.RecurringTransactionId.ToMaybe(),
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static TransactionGroup AsTransactionGroup(this List<TransactionEntity> entity)
        {
            var transactions = entity.Select(e => e.AsTransaction()).ToList();
            var transactionsWithCategory = transactions.Where(t => t.CategoryId.IsSome).ToList();

            return new TransactionGroup
            {
                TotalSum = transactions.Sum(t => t.Amount),
                Transactions = transactions,
                // Not using .ToLookup() as this will need a special JSON-converter.
                TransactionsPerCategory = transactionsWithCategory
                    .GroupBy(t => t.Category.Value, t => t, new CategoryComparer())
                    .ToDictionary(g => g.Key, g => g.ToList()),
                TransactionsPerType = transactions
                    .GroupBy(t => t.Type)
                    .ToDictionary(g => g.Key, g => g.ToList()),
            };
        }
    }
}