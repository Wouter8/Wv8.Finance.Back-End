namespace PersonalFinance.Business.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;

    /// <summary>
    /// A class containing extension methods relevant to intervals.
    /// </summary>
    public static class IntervalExtensions
    {
        /// <summary>
        /// Converts the intervals to the dates to be used in reports. This is always the start of the interval.
        /// </summary>
        /// <param name="intervals">The intervals.</param>
        /// <returns>The dates.</returns>
        public static List<LocalDate> ToDates(this List<DateInterval> intervals)
        {
            return intervals.Select(i => i.Start).ToList();
        }

        /// <summary>
        /// Filters the daily balances which fall within a provided range.
        /// </summary>
        /// <param name="dailyBalances">The daily balances to filter.</param>
        /// <param name="start">The inclusive start date.</param>
        /// <param name="end">The inclusive end date.</param>
        /// <returns>The filtered daily balances.</returns>
        public static List<DailyBalanceEntity> Within(
            this List<DailyBalanceEntity> dailyBalances, LocalDate start, LocalDate end)
        {
            var foundFirstRelevant = false;
            return dailyBalances
                .OrderByDescending(db => db.Date)
                .SkipWhile(db => db.Date > end)
                .TakeWhile(db =>
                {
                    if (foundFirstRelevant)
                        return false;

                    if (db.Date <= start)
                        foundFirstRelevant = true;

                    return true;
                })
                .OrderBy(db => db.Date)
                .ToList();
        }

        /// <summary>
        /// Converts a list of daily balances to a list of balance intervals.
        /// </summary>
        /// <param name="dailyBalances">The daily balances.</param>
        /// <returns>The list of balance intervals.</returns>
        public static List<BalanceInterval> ToBalanceIntervals(this List<DailyBalanceEntity> dailyBalances)
        {
            var intervalsByAccount = dailyBalances.GroupBy(db => db.AccountId)
                .ToDictionary(kv => kv.Key, kv => kv.ToList().ToIntervalsForOneAccount());

            var allIntervals = intervalsByAccount.Values.SelectMany(l => l).ToList();

            return allIntervals.Any()
                ? intervalsByAccount.Values.SelectMany(l => l).ToList().Merge()
                : new List<BalanceInterval>();
        }

        /// <summary>
        /// Creates a list of balance intervals where the first interval starts at <paramref name="start"/> and the
        /// last interval ends at <paramref name="end"/>. This means that intervals are either capped or added with a
        /// balance of 0. This also means that when <paramref name="balanceIntervals"/> does not contain an interval
        /// that overlaps with the given range, that a list is returned with one entry that spans the whole range.
        /// </summary>
        /// <param name="balanceIntervals">The balance intervals.</param>
        /// <param name="start">The first date to be included in the intervals.</param>
        /// <param name="end">The last date to be included in the intervals.</param>
        /// <returns>A list of balance intervals.</returns>
        public static List<BalanceInterval> ToFixedPeriod(
            this List<BalanceInterval> balanceIntervals, LocalDate start, LocalDate end)
        {
            var result = new List<BalanceInterval>();

            var firstEntry = balanceIntervals
                .OrderByDescending(bi => bi.Interval.Start)
                .FirstOrNone(bi => bi.Interval.Start <= start);
            var relevantEntries = balanceIntervals
                .Where(bi => bi.Interval.Start > start && bi.Interval.Start <= end)
                .OrderBy(bi => bi.Interval.Start)
                .ToList();

            BalanceInterval Cap(BalanceInterval bi)
            {
                var s = bi.Interval.Start <= start ? start : bi.Interval.Start;
                var e = bi.Interval.End >= end ? end : bi.Interval.End;
                return new BalanceInterval(s, e, bi.Balance);
            }

            var firstBalance = firstEntry.Select(e => e.Balance).ValueOrElse(0);
            var firstEnd = !relevantEntries.Any()
                ? end
                : firstEntry
                    .Select(e => e.Interval.End)
                    .ValueOrElse(relevantEntries
                        .FirstOrNone()
                        .Select(e => e.Interval.Start.PlusDays(-1))
                        .ValueOrElse(end));

            result.Add(new BalanceInterval(start, firstEnd, firstBalance));

            result.AddRange(relevantEntries);
            result = result.Select(Cap).ToList();

            return result;
        }

        /// <summary>
        /// Converts a list of balance intervals to a list of balance intervals where each interval is 1 day.
        /// </summary>
        /// <param name="intervals">The intervals.</param>
        /// <returns>A list of daily balance intervals.</returns>
        public static List<BalanceInterval> ToDailyIntervals(this List<BalanceInterval> intervals)
        {
            var result = new List<BalanceInterval>();

            foreach (var bi in intervals)
            {
                result.AddRange(bi.Interval.Select(i => new BalanceInterval(i, i, bi.Balance)));
            }

            return result;
        }

        /// <summary>
        /// Converts a list of daily balances to a list of balance intervals. Expects <paramref name="dailyBalances"/>
        /// to only contain balances of a single account.
        /// </summary>
        /// <param name="dailyBalances">The daily balances.</param>
        /// <returns>The list of balance intervals.</returns>
        private static List<BalanceInterval> ToIntervalsForOneAccount(this List<DailyBalanceEntity> dailyBalances)
        {
            var intervals = new List<BalanceInterval>();

            if (!dailyBalances.Any())
                return intervals;

            for (var i = dailyBalances.Count - 1; i >= 1; i--)
            {
                // The end date of a date interval is inclusive, so end at the day before the next interval starts.
                var end = dailyBalances[i].Date.PlusDays(-1);
                var intervalStart = dailyBalances[i - 1];

                intervals.Add(new BalanceInterval(intervalStart.Date, end, intervalStart.Balance));
            }

            var last = dailyBalances.Last();
            intervals.Add(new BalanceInterval(last.Date, DateTime.MaxValue.ToLocalDate(), last.Balance));

            intervals.Reverse();

            return intervals;
        }

        /// <summary>
        /// Combines overlapping balance intervals.
        /// </summary>
        /// <param name="balanceIntervals">The balance intervals to combine.</param>
        /// <returns>The list of combined balance intervals.</returns>
        private static List<BalanceInterval> Merge(this List<BalanceInterval> balanceIntervals)
        {
            // Store the changes for each interval. On the start the balance gets added, after the end the balance gets
            // removed. This way consecutive intervals will have 1 change between intervals.
            var changes = new Dictionary<LocalDate, decimal>();

            var maxDate = balanceIntervals.Max(bi => bi.Interval.End);

            foreach (var bi in balanceIntervals)
            {
                var addDate = bi.Interval.Start;
                var existingStartChanges = changes.TryGetValue(addDate).ValueOrElse(0);
                changes[addDate] = existingStartChanges + bi.Balance;

                // Adding the remove change for the last interval is not needed as we don't want to start a new interval
                // for that date. This also fixes exceptions that would arise when adding a day to the max date.
                if (bi.Interval.End != maxDate)
                {
                    var removeDate = bi.Interval.End.PlusDays(1); // This will break for DateTime.MaxValue dates
                    var existingEndChanges = changes.TryGetValue(removeDate).ValueOrElse(0);
                    changes[removeDate] = existingEndChanges - bi.Balance;
                }
            }

            // Order the changes
            var orderedChanges = changes.OrderBy(c => c.Key).ToList();

            // The start and balance for the next interval, these are updated while looping over the changes.
            var (previousEnd, previousBalance) = orderedChanges.First();

            var mergedIntervals = new List<BalanceInterval>();

            // Loop over all changes and create an interval for each one. Skip the first entry as that marks the start
            // of the first interval.
            for (var i = 1; i < orderedChanges.Count; i++)
            {
                var end = orderedChanges[i];

                mergedIntervals.Add(new BalanceInterval(previousEnd, end.Key.PlusDays(-1), previousBalance));

                var newBalance = previousBalance + end.Value;

                previousEnd = end.Key;
                previousBalance = newBalance;
            }

            mergedIntervals.Add(new BalanceInterval(previousEnd, maxDate, previousBalance));

            return mergedIntervals;
        }
    }
}