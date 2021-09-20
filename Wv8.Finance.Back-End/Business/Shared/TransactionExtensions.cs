namespace PersonalFinance.Business.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Data.Models;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class containing extension methods related to transactions.
    /// </summary>
    public static class TransactionExtensions
    {
        /// <summary>
        /// Maps the provided transactions for each provided interval.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <param name="intervals">The intervals.</param>
        /// <param name="mapping">The mapping function.</param>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <returns>Per interval the result of the mapping function for the relevant transactions.</returns>
        public static List<T> MapTransactionsPerInterval<T>(
            this List<TransactionEntity> transactions,
            List<DateInterval> intervals,
            Func<List<TransactionEntity>, T> mapping)
        {
            var groupedTransactions = transactions
                .Select(t => new { Interval = intervals.SingleOrNone(i => i.Contains(t.Date)), Transaction = t })
                .Where(t => t.Interval.IsSome);

            return intervals
                .Select(i => mapping(
                    groupedTransactions
                        .Where(t => t.Interval == i)
                        .Select(t => t.Transaction)
                        .ToList()))
                .ToList();
        }

        /// <summary>
        /// Sums the amount of the transactions.
        /// </summary>
        /// <param name="transactions">The transactions.</param>
        /// <param name="usePersonalAmount">If <c>true</c> <see cref="TransactionEntity.PersonalAmount"/> is used
        /// instead of <see cref="TransactionEntity.Amount"/>.</param>
        /// <returns>The sum of the amount.</returns>
        public static decimal Sum(this List<TransactionEntity> transactions, bool usePersonalAmount)
        {
            return transactions.Sum(t => usePersonalAmount ? t.PersonalAmount : t.Amount);
        }
    }
}