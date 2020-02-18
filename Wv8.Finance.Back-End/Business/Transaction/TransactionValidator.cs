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
        /// Validates that the interval provided is in the correct range.
        /// </summary>
        /// <param name="input">The input.</param>
        public void Interval(int input)
        {
            if (input <= 0)
                throw new ValidationException("The interval must be greater than 0.");
        }

        /// <summary>
        /// Validates that the correct fields are provided for each transaction type.
        /// </summary>
        /// <param name="type">The transaction type.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="receivingAccountId">The receiving account identifier.</param>
        public void Type(TransactionType type, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            switch (type)
            {
                case TransactionType.Expense:
                    if (amount > 0)
                        throw new ValidationException($"The amount has to be negative.");
                    if (categoryId.IsNone)
                        throw new ValidationException($"A category has to be specified for an income or expense transaction.");
                    break;
                case TransactionType.Income:
                    if (amount <= 0)
                        throw new ValidationException($"The amount has to be greater than 0.");
                    if (categoryId.IsNone)
                        throw new ValidationException($"A category has to be specified for an income or expense transaction.");
                    break;
                case TransactionType.Transfer:
                    if (amount <= 0)
                        throw new ValidationException($"The amount has to be greater than 0.");
                    if (receivingAccountId.IsNone)
                        throw new ValidationException($"A receiving account has to be specified for a transfer transaction.");
                    break;
                default:
                    throw new InvalidOperationException($"Unknown transaction type.");
            }
        }
    }
}