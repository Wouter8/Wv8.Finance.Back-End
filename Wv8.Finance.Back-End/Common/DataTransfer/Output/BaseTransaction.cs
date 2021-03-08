namespace PersonalFinance.Common.DataTransfer.Output
{
    using System.Collections.Generic;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// A class for a data transfer object representing a base transaction.
    /// </summary>
    public class BaseTransaction
    {
        /// <summary>
        /// The identifier of this transaction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of this transaction.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of this transaction.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// The amount of this transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The identifier of the category this transaction belongs to.
        /// This value is not set for transfer transactions.
        /// </summary>
        public Maybe<int> CategoryId { get; set; }

        /// <summary>
        /// the category this transaction belongs to.
        /// This is not set for transfer transactions.
        /// </summary>
        public Maybe<Category> Category { get; set; }

        /// <summary>
        /// The identifier of the account this transaction belongs to.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The account this transaction belongs to.
        /// </summary>
        public Account Account { get; set; }

        /// <summary>
        /// The identifier of the account that is the receiver.
        /// Only has a value when this transaction is a transfer transaction.
        /// </summary>
        public Maybe<int> ReceivingAccountId { get; set; }

        /// <summary>
        /// The account that is the receiver.
        /// Only has a value when this transaction is a transfer transaction.
        /// </summary>
        public Maybe<Account> ReceivingAccount { get; set; }

        /// <summary>
        /// A value indicating if this transaction needs to be manually confirmed before being processed.
        /// This can be useful when the exact date or amount is not known.
        /// </summary>
        public bool NeedsConfirmation { get; set; }

        /// <summary>
        /// The collection of split details which are linked to this transaction.
        /// </summary>
        public List<SplitDetail> SplitDetails { get; set; }

        /// <summary>
        /// The collection of payment requests which are linked to this transaction.
        /// </summary>
        public List<PaymentRequest> PaymentRequests { get; set; }

        /// <summary>
        /// The personal amount (amount - payment requests - splits) of the transaction.
        /// </summary>
        public decimal PersonalAmount { get; set; }
    }
}