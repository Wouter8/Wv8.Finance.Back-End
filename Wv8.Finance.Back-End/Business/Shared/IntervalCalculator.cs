namespace PersonalFinance.Business.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A static class with methods relevant to calculating an interval.
    /// </summary>
    public static class IntervalCalculator
    {
        /// <summary>
        /// Gets the intervals within a given period, with a maximum amount of intervals.
        /// </summary>
        /// <param name="start">The inclusive start of the period.</param>
        /// <param name="end">The inclusive end of the period.</param>
        /// <param name="maxIntervals">The maximum amount of intervals.</param>
        /// <returns>A tuple containing the interval unit and the list of intervals.</returns>
        public static (ReportIntervalUnit, List<DateInterval>) GetIntervals(LocalDate start, LocalDate end, int maxIntervals)
        {
            var exclusiveEnd = end.PlusDays(1);
            var days = Period.Between(start, exclusiveEnd, PeriodUnits.Days).Days;
            if (days <= maxIntervals)
            {
                return (ReportIntervalUnit.Days, DateIntervals(start, exclusiveEnd, Period.FromDays(1)));
            }

            var weeks = Period.Between(start, exclusiveEnd, PeriodUnits.Weeks).Weeks;
            if (weeks <= maxIntervals)
            {
                return (ReportIntervalUnit.Weeks, DateIntervals(start, exclusiveEnd, Period.FromWeeks(1)));
            }

            var months = Period.Between(start, exclusiveEnd, PeriodUnits.Months).Months;
            if (months <= maxIntervals)
            {
                return (ReportIntervalUnit.Months, DateIntervals(start, exclusiveEnd, Period.FromMonths(1)));
            }

            var years = Period.Between(start, exclusiveEnd, PeriodUnits.Years).Years;
            if (years <= maxIntervals)
            {
                return (ReportIntervalUnit.Years, DateIntervals(start, exclusiveEnd, Period.FromYears(1)));
            }

            throw new InvalidOperationException(
                $"Not able to create a maximum of {maxIntervals} intervals between {start:dd-MM-yyyy} and {end:dd-MM-yyyy}.");
        }

        /// <summary>
        /// Gets the intervals within a given period. Opposed to <see cref="GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>
        /// this does not get the smallest interval with which a certain number of intervals is satisfied, it rather uses a
        /// pre-defined approach which makes the most sense for reports.
        /// * 1 day - 1 month (inclusive): per day.
        /// * 1 month - 6 months (exclusive): per week.
        /// * 6 months - 3 years (inclusive): per month.
        /// * 3 years - ...: per year.
        /// </summary>
        /// <param name="start">The inclusive start of the period.</param>
        /// <param name="end">The inclusive end of the period.</param>
        /// <returns>A tuple containing the interval unit and the list of intervals.</returns>
        public static (ReportIntervalUnit, List<DateInterval>) GetIntervals(LocalDate start, LocalDate end)
        {
            var exclusiveEnd = end.PlusDays(1);
            var period = Period.Between(start, exclusiveEnd, PeriodUnits.Months | PeriodUnits.Days);

            if (period.Months < 1 || (period.Months == 1 && period.Days == 0))
                return (ReportIntervalUnit.Days, DateIntervals(start, exclusiveEnd, Period.FromDays(1)));

            if (period.Months < 6)
                return (ReportIntervalUnit.Weeks, DateIntervals(start, exclusiveEnd, Period.FromWeeks(1)));

            const int threeYears = 3 * 12;
            if (period.Months < threeYears || (period.Months == threeYears && period.Days == 0))
                return (ReportIntervalUnit.Months, DateIntervals(start, exclusiveEnd, Period.FromMonths(1)));

            return (ReportIntervalUnit.Years, DateIntervals(start, exclusiveEnd, Period.FromYears(1)));
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
        private static List<DateInterval> DateIntervals(LocalDate start, LocalDate end, Period period)
        {
            return start.DateBetweenPerInterval(end, period).ToList().ToIntervals();
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
