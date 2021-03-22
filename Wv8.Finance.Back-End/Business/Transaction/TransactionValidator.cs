namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.External.Splitwise;
    using Wv8.Core.EntityFramework;
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
        /// Validates that the account type is normal.
        /// </summary>
        /// <param name="type">The account type to validate.</param>
        public void AccountType(AccountType type)
        {
            if (type == Common.Enums.AccountType.Splitwise)
                throw new ValidationException("It is not possible manually create or update Splitwise transactions.");
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
        /// Validates a list of payment requests and Splitwise splits.
        /// </summary>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        /// <param name="paymentRequests">The list of payment requests.</param>
        /// <param name="splitwiseSplits">The list of Splitwise splits.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="totalAmount">The amount of the transaction.</param>
        public void Splits(
            ISplitwiseContext splitwiseContext,
            List<InputPaymentRequest> paymentRequests,
            List<InputSplitwiseSplit> splitwiseSplits,
            TransactionType type,
            decimal totalAmount)
        {
            if (!paymentRequests.Any() && !splitwiseSplits.Any())
                return;

            totalAmount = Math.Abs(totalAmount);
            var sumPaymentRequests = paymentRequests.Sum(pr => pr.Count * pr.Amount);
            var sumSplitwiseSplits = splitwiseSplits.Sum(s => s.Amount);
            var personalAmount = totalAmount - sumSplitwiseSplits;

            if (type != TransactionType.Expense)
            {
                throw new ValidationException(
                    "Payment requests and Splitwise splits can only be specified on expenses.");
            }

            if (sumSplitwiseSplits > totalAmount)
            {
                throw new ValidationException(
                    "The amount split can not exceed the total amount of the transaction.");
            }
            if (sumPaymentRequests > personalAmount)
            {
                throw new ValidationException(
                    $"The amount of the payment requests ({sumPaymentRequests}) can not exceed the personal " +
                    $"amount ({personalAmount})");
            }

            if (splitwiseSplits.Any(s => s.Amount <= 0))
                throw new ValidationException("Splits must have an amount greater than 0.");

            var inputtedUserIds = splitwiseSplits.Select(s => s.UserId).ToSet();
            if (inputtedUserIds.Count != splitwiseSplits.Count)
                throw new ValidationException("A user can only be linked to a single split.");

            var splitwiseUserIds = splitwiseContext.GetUsers().Select(u => u.Id).ToSet();
            if (!inputtedUserIds.IsSubsetOf(splitwiseUserIds))
                throw new ValidationException("Unknown Splitwise user(s) specified.");

            foreach (var paymentRequest in paymentRequests)
            {
                if (paymentRequest.Count <= 0)
                    throw new ValidationException("A payment request must at least have 1 requested payment.");

                if (string.IsNullOrWhiteSpace(paymentRequest.Name))
                    throw new ValidationException("A payment request must specify a name.");

                if (paymentRequest.Amount <= 0)
                    throw new ValidationException("A payment request must have an amount greater than 0.");
            }
        }
    }
}