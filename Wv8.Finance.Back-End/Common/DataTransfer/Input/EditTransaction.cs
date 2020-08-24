namespace PersonalFinance.Common.DataTransfer.Input
{
    using System.Collections.Generic;
    using Wv8.Core;

    /// <summary>
    /// The input to edit a transaction.
    /// </summary>
    public class EditTransaction
    {
        /// <summary>
        /// The identifier of the transaction to be edited.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The new account identifier.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The new description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The new date.
        /// </summary>
        public string DateString { get; set; }

        /// <summary>
        /// The new amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The new category identifier.
        /// </summary>
        public Maybe<int> CategoryId { get; set; }

        /// <summary>
        /// The new receiving account identifier.
        /// </summary>
        public Maybe<int> ReceivingAccountId { get; set; }

        /// <summary>
        /// The new collection of payment requests.
        /// </summary>
        public List<InputPaymentRequest> PaymentRequests { get; set; }
    }
}