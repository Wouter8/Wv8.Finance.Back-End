namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.Enums;
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
        /// Validates that the amount is valid for the type of transaction.
        /// </summary>
        /// <param name="amount">The amount input.</param>
        /// <param name="type">The type of the transaction.</param>
        public void Amount(decimal amount, TransactionType type)
        {
            if (type == TransactionType.Expense)
            {
                if (amount >= 0)
                    throw new ValidationException("An expense transaction must have a negative amount.");

                return;
            }

            if (amount <= 0)
            {
                throw new ValidationException("Only expense transactions can have negative amounts.");
            }
        }

        /// <summary>
        /// Validates a list of payment requests.
        /// </summary>
        /// <param name="paymentRequests">The list of payment requests.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="transactionAmount">The amount of the transaction.</param>
        public void PaymentRequests(
            List<InputPaymentRequest> paymentRequests, TransactionType type, decimal transactionAmount)
        {
            if (!paymentRequests.Any())
                return;

            if (type != TransactionType.Expense)
                throw new ValidationException("Payment requests can only be specified on expenses.");

            var sum = paymentRequests.Sum(pr => pr.Count * pr.Amount);
            var absTransactionAmount = Math.Abs(transactionAmount);
            if (sum > absTransactionAmount)
            {
                throw new ValidationException($"The amount of the payment requests ({-sum}) can not exceed the " +
                                              $"transaction amount ({transactionAmount})");
            }

            foreach (var paymentRequest in paymentRequests)
            {
                if (paymentRequest.Count <= 0)
                    throw new ValidationException("A payment request must at least need 1 payment.");

                if (string.IsNullOrWhiteSpace(paymentRequest.Name))
                    throw new ValidationException("A payment request must specify a name.");

                if (paymentRequest.Amount <= 0)
                    throw new ValidationException("A payment request must have an amount greater than 0.");
            }
        }
    }
}