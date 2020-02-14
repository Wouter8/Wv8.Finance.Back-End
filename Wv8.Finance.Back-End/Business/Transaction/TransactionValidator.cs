namespace PersonalFinance.Business.Transaction
{
    using System;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The validator for all fields related to transactions.
    /// </summary>
    public class TransactionValidator : BaseValidator
    {
        private const int minDescriptionLength = 3;
        private const int maxDescriptionLength = 32;

        /// <summary>
        /// Validates and normalizes the description of an account.
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
        /// Validates the amount of a budget.
        /// </summary>
        /// <param name="amount">input.</param>
        public void Amount(decimal amount)
        {
            if (amount <= 0)
                throw new ValidationException("The amount of the transaction has to be greater than zero.");
        }

        /// <summary>
        /// Validates that the correct fields are provided for each transaction type.
        /// </summary>
        /// <param name="type">The transaction type.</param>
        /// <param name="categoryId">The category identifier.</param>
        public void Type(TransactionType type, Maybe<int> categoryId)
        {
            switch (type)
            {
                case TransactionType.Expense:
                case TransactionType.Income:
                    if (categoryId.IsNone)
                        throw new ValidationException($"A category has to be specified for an income or expense transaction.");
                    break;
                case TransactionType.Transfer:
                    break;
                default:
                    throw new InvalidOperationException($"Unknown transaction type.");
            }
        }
    }
}