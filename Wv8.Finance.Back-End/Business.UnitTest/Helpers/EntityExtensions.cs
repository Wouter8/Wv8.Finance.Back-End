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