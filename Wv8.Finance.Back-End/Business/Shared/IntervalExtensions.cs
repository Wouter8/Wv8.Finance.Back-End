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
    }
}