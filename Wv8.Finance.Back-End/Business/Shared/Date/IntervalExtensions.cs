namespace PersonalFinance.Business.Shared.Date
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;

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
    }
}