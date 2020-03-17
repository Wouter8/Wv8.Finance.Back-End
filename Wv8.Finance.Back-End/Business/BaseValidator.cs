namespace PersonalFinance.Business
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using NodaTime;
    using NodaTime.Text;
    using Wv8.Core;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// Base class for specific validators providing base functionality.
    /// </summary>
    public abstract class BaseValidator
    {
        /// <summary>
        /// Validates that a provided icon pack and icon name is valid.
        /// </summary>
        /// <param name="iconPack">The icon pack input.</param>
        /// <param name="iconName">The icon name input.</param>
        /// <param name="iconColor">The icon background color input.</param>
        public void Icon(string iconPack, string iconName, string iconColor)
        {
            this.NotEmpty(iconPack, nameof(iconPack));
            this.NotEmpty(iconName, nameof(iconName));
            this.NotEmpty(iconName, nameof(iconColor));

            this.InRange(iconPack, 1, 3, nameof(iconPack));
            this.InRange(iconColor, 7, 7, nameof(iconColor));
        }

        /// <summary>
        /// Validates that a string can be converted to a date and returns the converted value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The converted date.</returns>
        public LocalDate DateString(string input, string parameterName)
        {
            // First convert to DateTime, then to LocalDate.
            var success = DateTime.TryParse(input, new CultureInfo("en-US"), DateTimeStyles.None, out var dateTime);
            if (!success)
                throw new ValidationException($"String for {parameterName} could not be converted to a date");

            var date = LocalDate.FromDateTime(dateTime);
            return date;
        }

        /// <summary>
        /// Validates a period.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <param name="canBeEqual">A value indicating if the start and end can be equal.</param>
        public void Period(LocalDate start, LocalDate end, bool canBeEqual = false)
        {
            if (canBeEqual)
            {
                if (start > end)
                    throw new ValidationException("Start date has to be equal to or before the end date.");
            }
            else
            {
                if (start >= end)
                    throw new ValidationException("Start date has to be before the end date.");
            }
        }

        /// <summary>
        /// Validates a period.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <param name="canBeEqual">A value indicating if the start and end can be equal.</param>
        public void Period(Maybe<LocalDate> start, Maybe<LocalDate> end, bool canBeEqual = false)
        {
            if (start.IsSome != end.IsSome)
                throw new ValidationException($"Both start and end of the period have to be specified.");

            if (start.IsNone && end.IsNone)
                return;

            if (canBeEqual)
            {
                if (start.Value > end.Value)
                    throw new ValidationException("Start date has to be equal to or before the end date.");
            }
            else
            {
                if (start.Value >= end.Value)
                    throw new ValidationException("Start date has to be before the end date.");
            }
        }

        /// <summary>
        /// Validates that the pagination parameters are properly filled.
        /// </summary>
        /// <param name="skip">The skip value.</param>
        /// <param name="take">The take value.</param>
        public void Pagination(int skip, int take)
        {
            if (skip < 0)
                throw new ValidationException($"The value for skip can not be less than zero.");

            if (take <= 0)
                throw new ValidationException($"The value for take can not be less than or equal to zero.");
        }

        /// <summary>
        /// Validates that a given value is not null.
        /// </summary>
        /// <typeparam name="T">The type of the object, must be nullable.</typeparam>
        /// <param name="input">The input to be validated.</param>
        /// <param name="parameterName">The parameter name.</param>
        protected void NotNull<T>(T input, string parameterName)
            where T : class
        {
            if (input == null)
                throw new ValidationException($"Value for {parameterName} can not be null.");
        }

        /// <summary>
        /// Validates an input does not exceed a maximum length. Adds the possibility to trim to the max length.
        /// </summary>
        /// <param name="input">The input to be validated.</param>
        /// <param name="maxLength">The maximum length of the input.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="trimRight">If true, the input will be trimmed to the max length.</param>
        /// <returns>The validated input.</returns>
        protected string MaxLength(string input, int maxLength, string parameterName, bool trimRight = false)
        {
            this.NotNull(input, parameterName);

            if (input.Length <= maxLength) return input;

            if (trimRight)
                return input.Substring(0, maxLength);

            throw new ValidationException($"Value for {parameterName} exceeds the maximum length of {maxLength}.");
        }

        /// <summary>
        /// Validates that a collection does not exceed a maximum length.
        /// </summary>
        /// <typeparam name="T">The type in the collection.</typeparam>
        /// <param name="input">The collection to be validated.</param>
        /// <param name="maxLength">The maximum length of the collection.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="trimExceeding">If true, only the maximum amount of objects are returned.
        /// Objects after that are discarded.</param>
        /// <returns>The input, or the collection with the maximum amount of elements.</returns>
        protected ICollection<T> MaxLength<T>(ICollection<T> input, int maxLength, string parameterName, bool trimExceeding = false)
        {
            this.NotNull(input, parameterName);

            if (input.Count <= maxLength) return input;

            if (trimExceeding)
                return input.Take(maxLength).ToList();

            throw new ValidationException($"Value for {parameterName} exceeds the maximum length of {maxLength}.");
        }

        /// <summary>
        /// Validates that a collection is not empty.
        /// </summary>
        /// <typeparam name="T">The type in the collection.</typeparam>
        /// <param name="input">The collection to be validated.</param>
        /// <param name="parameterName">The parameter name.</param>
        protected void NoNullEntry<T>(ICollection<T> input, string parameterName)
        {
            this.NotNull(input, parameterName);

            if (!input.Any())
                throw new ValidationException($"Value for {parameterName} can not be empty.");
        }

        /// <summary>
        /// Validates that a collection is not empty.
        /// </summary>
        /// <typeparam name="T">The type in the collection.</typeparam>
        /// <param name="input">The collection to be validated.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="allowNull">Value indicating if a null value should be counted.</param>
        protected void NotEmpty<T>(ICollection<T> input, string parameterName, bool allowNull = false)
        {
            this.NotNull(input, parameterName);

            if (!input
                .WhereIf(!allowNull, v => v != null)
                .Any())
                throw new ValidationException($"Value for {parameterName} can not be empty.");
        }

        /// <summary>
        /// Validates that a string contains at least one character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="parameterName">The parameter name.</param>
        protected void NotEmpty(string input, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ValidationException($"Value for {parameterName} can not be empty.");
        }

        /// <summary>
        /// Validates an integer value to be within a specified range.
        /// Note that the min and max values are also valid.
        /// </summary>
        /// <param name="input">The input to be validated.</param>
        /// <param name="min">The minimum value of the input.</param>
        /// <param name="max">The maximum value of the input.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="defaultValue">An optional default value if the input is not in the range.</param>
        /// <returns>The input, or the default value if it is provided.</returns>
        protected int InRange(int input, int min, int max, string parameterName, int? defaultValue = null)
        {
            if (input >= min && input <= max) return input;

            if (defaultValue.HasValue) return defaultValue.Value;

            throw new ValidationException($"Value for {parameterName} is not within range {min}-{max}.");
        }

        /// <summary>
        /// Validates a string value to be within a specified length.
        /// Note that the min and max values are also valid.
        /// </summary>
        /// <param name="input">The input to be validated.</param>
        /// <param name="min">The minimum length of the input.</param>
        /// <param name="max">The maximum length of the input.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <param name="defaultValue">An optional default value if the input is not in the range.</param>
        /// <returns>The input, or the default value if it is provided.</returns>
        protected string InRange(string input, int min, int max, string parameterName, string defaultValue = null)
        {
            if (input.Length >= min && input.Length <= max) return input;

            var maybe = new Maybe<string>(defaultValue);
            if (maybe.IsSome) return maybe.Value;

            throw new ValidationException($"Length for {parameterName} is not within range {min}-{max}.");
        }
    }
}