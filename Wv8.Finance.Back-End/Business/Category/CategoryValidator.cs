namespace PersonalFinance.Business.Category
{
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The validator for all fields related to categories.
    /// </summary>
    public class CategoryValidator : BaseValidator
    {
        private const int minDescriptionLength = 3;
        private const int maxDescriptionLength = 32;

        /// <summary>
        /// Validates and normalizes the description of an category.
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
        /// Validates that the expected monthly amount is valid for a type.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="type">The type.</param>
        public void ExpectedMonthlyAmount(Maybe<decimal> input, CategoryType type)
        {
            if (input.IsNone)
                return;

            switch (type)
            {
                case CategoryType.Expense:
                    if (input.Value >= 0)
                        throw new ValidationException($"Expected monthly amount must be negative for an expense category.");
                    break;
                case CategoryType.Income:
                    if (input.Value <= 0)
                        throw new ValidationException($"Expected monthly amount must be positive for an income category.");
                    break;
            }
        }
    }
}