﻿namespace PersonalFinance.Common
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using NodaTime;
    using Wv8.Core;

    /// <summary>
    /// A class providing extension methods for a local date object.
    /// </summary>
    public static class LocalDateExtensions
    {
        /// <summary>
        /// Converts a date to a string in a normal format.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <returns>The string.</returns>
        public static string ToDateString(this LocalDate date)
        {
            return date.ToString("d", new CultureInfo("en-US"));
        }

        /// <summary>
        /// Converts a list of dates to a list of strings in a normal format.
        /// </summary>
        /// <param name="dates">The dates to be converted.</param>
        /// <returns>The strings.</returns>
        public static List<string> ToDateStrings(this List<LocalDate> dates)
        {
            return dates.Select(ToDateString).ToList();
        }

        /// <summary>
        /// Converts a date to a string in a normal format.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <returns>The string.</returns>
        public static Maybe<string> ToDateString(this LocalDate? date)
        {
            return date.ToMaybe().Select(d => d.ToDateString());
        }

        /// <summary>
        /// Converts a date to a string in a normal format.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <returns>The string.</returns>
        public static string ToDateString(this DateTime date)
        {
            return date.ToLocalDate().ToDateString();
        }

        /// <summary>
        /// Converts a date to a string in a normal format.
        /// </summary>
        /// <param name="date">The date to be converted.</param>
        /// <returns>The string.</returns>
        public static Maybe<string> ToDateString(this DateTime? date)
        {
            return date.ToMaybe().Select(d => d.ToDateString());
        }

        /// <summary>
        /// Converts a date time to a string in a normal format.
        /// </summary>
        /// <param name="date">The date time to be converted.</param>
        /// <returns>The string.</returns>
        public static string ToDateTimeString(this DateTime date)
        {
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return date.ToString("O");
        }

        /// <summary>
        /// Converts a date time object to a local date object.
        /// </summary>
        /// <param name="dateTime">The date time object.</param>
        /// <returns>The local date.</returns>
        public static LocalDate ToLocalDate(this DateTime dateTime)
        {
            return LocalDate.FromDateTime(dateTime);
        }

        /// <summary>
        /// Converts a date time object to a local date object.
        /// </summary>
        /// <param name="dateTime">The date time object.</param>
        /// <returns>The local date.</returns>
        public static LocalDate? ToLocalDate(this DateTime? dateTime)
        {
            return dateTime.ToMaybe().Select(LocalDate.FromDateTime).ToNullable();
        }

        /// <summary>
        /// Converts a local date to a date time in UTC. The date time will have the same date as the local date, and
        /// have its time set to midnight.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>The date time.</returns>
        public static DateTime ToDateTimeUtc(this LocalDate date)
        {
            return date.AtStartOfDayInZone(DateTimeZone.Utc).ToDateTimeUtc();
        }
    }
}
