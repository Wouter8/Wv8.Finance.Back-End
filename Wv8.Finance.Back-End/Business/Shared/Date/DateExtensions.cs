namespace PersonalFinance.Business.Shared.Date
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common;

    /// <summary>
    /// A class containing extension methods relevant to dates.
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Gets the first date of the month of the provided date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The date which is the first date of the month.</returns>
        public static LocalDate FirstDateOfMonth(this LocalDate date)
        {
            return new LocalDate(date.Year, date.Month, 1);
        }

        /// <summary>
        /// Gets the last date of the month of the provided date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The date which is the last date of the month.</returns>
        public static LocalDate LastDateOfMonth(this LocalDate date)
        {
            return new LocalDate(date.Year, date.Month, 1).PlusMonths(1);
        }

        /// <summary>
        /// Converts a period to a list of intervals. The first interval will start at <paramref name="start"/>. All
        /// intervals will have the length of <paramref name="period"/>, except for the last interval which will
        /// optionally be capped at <paramref name="end"/>.
        /// </summary>
        /// <param name="start">The start of the period.</param>
        /// <param name="end">The end of the period.</param>
        /// <param name="period">The length of each interval.</param>
        /// <returns>The list of intervals.</returns>
        public static List<DateInterval> ToDateIntervals(this LocalDate start, LocalDate end, Period period)
        {
            return start.DateBetweenPerInterval(end, period).ToList().ToIntervals();
        }

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
        /// Converts a list of dates to intervals.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <returns>The intervals.</returns>
        private static List<DateInterval> ToIntervals(this List<LocalDate> dates)
        {
            var intervals = new List<DateInterval>();
            for (var i = dates.Count - 1; i >= 1; i--)
            {
                // The end date of a date interval is inclusive, so end at the day before the next interval starts.
                var end = dates[i].PlusDays(-1);
                var start = dates[i - 1];

                intervals.Add(new DateInterval(start, end));
            }

            intervals.Reverse();

            return intervals;
        }

        private static IEnumerable<LocalDate> DateBetweenPerInterval(this LocalDate start, LocalDate end, Period period)
        {
            yield return start;
            var current = start;

            var done = false;

            while (!done)
            {
                current = current.Plus(period);

                if (current >= end)
                {
                    done = true;
                    current = end;
                }

                yield return current;
            }
        }
    }
}