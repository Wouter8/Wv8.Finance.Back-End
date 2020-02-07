namespace PersonalFinance.Business.Budget
{
    using System;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The validator for all fields related to budgets.
    /// </summary>
    public class BudgetValidator : BaseValidator
    {
        private const int minDescriptionLength = 3;
        private const int maxDescriptionLength = 32;

        /// <summary>
        /// Validates and normalizes the description of an budget.
        /// </summary>
        /// <param name="description">The input.</param>
        /// <returns>The normalized input.</returns>
        public string Description(string description)
        {
            this.NotEmpty(description, nameof(description));

            description = description.Trim();

            this.InRange(description, minDescriptionLength, maxDescriptionLength, nameof(description));

            return description;
        }

        /// <summary>
        /// Validates the period of a budget.
        /// </summary>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        public void Period(DateTime start, DateTime end)
        {
            if (start >= end)
                throw new ValidationException("Start date has to be before the end date.");
        }

        /// <summary>
        /// Validates the amount of a budget.
        /// </summary>
        /// <param name="amount">input.</param>
        public void Amount(decimal amount)
        {
            if (amount <= 0)
                throw new ValidationException("The amount of the budget has to be greater than zero.");
        }
    }
}