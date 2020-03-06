namespace PersonalFinance.Common
{
    using System;
    using System.Globalization;

    /// <summary>
    /// A class containing extension methods for <see cref="DateTime"/> objects.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts a date time object to an ISO string.
        /// </summary>
        /// <param name="dateTime">The object.</param>
        /// <returns>The ISO string.</returns>
        public static string ToIsoString(this DateTime dateTime)
        {
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            return dateTime.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}