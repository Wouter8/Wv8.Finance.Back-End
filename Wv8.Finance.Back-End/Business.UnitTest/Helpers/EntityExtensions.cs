namespace Business.UnitTest.Helpers
{
    using System.Linq;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// A class containing extensions methods for database entities.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Converts a <see cref="TransactionEntity"/> to a <see cref="InputTransaction"/>.
        /// </summary>
        /// <param name="transaction">The entity.</param>
        /// <returns>The converted object.</returns>
        public static InputTransaction ToInput(this TransactionEntity transaction)
        {
            return new InputTransaction
            {
                Amount = transaction.Amount,
                Description = transaction.Description,
                DateString = transaction.Date.ToDateString(),
                AccountId = transaction.AccountId,
                CategoryId = transaction.CategoryId.ToMaybe(),
                ReceivingAccountId = transaction.ReceivingAccountId.ToMaybe(),
                PaymentRequests = transaction.PaymentRequests.Select(pr => pr.ToInput()).ToList(),
                SplitwiseSplits = transaction.SplitDetails.Select(pr => pr.ToInput()).ToList(),
            };
        }

        /// <summary>
        /// Converts a <see cref="RecurringTransactionEntity"/> to a <see cref="InputRecurringTransaction"/>.
        /// </summary>
        /// <param name="transaction">The entity.</param>
        /// <returns>The converted object.</returns>
        public static InputRecurringTransaction ToInput(this RecurringTransactionEntity transaction)
        {
            return new InputRecurringTransaction
            {
                Amount = transaction.Amount,
                Description = transaction.Description,
                StartDateString = transaction.StartDate.ToDateString(),
                EndDateString = transaction.EndDate.ToDateString(),
                AccountId = transaction.AccountId,
                CategoryId = transaction.CategoryId.ToMaybe(),
                ReceivingAccountId = transaction.ReceivingAccountId.ToMaybe(),
                PaymentRequests = transaction.PaymentRequests.Select(pr => pr.ToInput()).ToList(),
                SplitwiseSplits = transaction.SplitDetails.Select(pr => pr.ToInput()).ToList(),
                Interval = transaction.Interval,
                IntervalUnit = transaction.IntervalUnit,
                NeedsConfirmation = transaction.NeedsConfirmation,
            };
        }

        /// <summary>
        /// Converts a <see cref="SplitDetailEntity"/> to a <see cref="InputSplitwiseSplit"/>.
        /// </summary>
        /// <param name="splitDetail">The entity.</param>
        /// <returns>The converted object.</returns>
        public static InputSplitwiseSplit ToInput(this SplitDetailEntity splitDetail)
        {
            return new InputSplitwiseSplit
            {
                UserId = splitDetail.SplitwiseUserId,
                Amount = splitDetail.Amount,
            };
        }

        /// <summary>
        /// Converts a <see cref="PaymentRequestEntity"/> to a <see cref="InputPaymentRequest"/>.
        /// </summary>
        /// <param name="paymentRequest">The entity.</param>
        /// <returns>The converted object.</returns>
        public static InputPaymentRequest ToInput(this PaymentRequestEntity paymentRequest)
        {
            return new InputPaymentRequest
            {
                Id = paymentRequest.Id,
                Amount = paymentRequest.Amount,
                Count = paymentRequest.Count,
                Name = paymentRequest.Name,
            };
        }
    }
}