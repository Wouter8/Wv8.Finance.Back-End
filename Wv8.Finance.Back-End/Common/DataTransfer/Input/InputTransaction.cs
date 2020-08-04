namespace PersonalFinance.Common.DataTransfer.Input
{
    using System.Collections.Generic;
    using Wv8.Core;

    /// <summary>
    /// A class containing user input with which a transaction can be created.
    /// </summary>
    public class InputTransaction
    {
        /// <summary>
        /// The account identifier.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date of the transaction.
        /// </summary>
        public string DateString { get; set; }

        /// <summary>
        /// The amount of the transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The category identifier.
        /// </summary>
        public Maybe<int> CategoryId { get; set; }

        /// <summary>
        /// The receiving account identifier.
        /// </summary>
        public Maybe<int> ReceivingAccountId { get; set; }

        /// <summary>
        /// <c>true</c> if the transaction needs to be confirmed before being processed, <c>false</c> otherwise.
        /// </summary>
        public bool NeedsConfirmation { get; set; }

        /// <summary>
        /// The collection of payment requests that are linked to this transaction.
        /// </summary>
        public List<InputPaymentRequest> PaymentRequests { get; set; }
    }
}