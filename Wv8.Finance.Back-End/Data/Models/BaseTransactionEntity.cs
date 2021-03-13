namespace PersonalFinance.Data.Models
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A base class for a transaction. This can be a recurring transaction or a normal transaction.
    /// </summary>
    public class BaseTransactionEntity
    {
        /// <summary>
        /// The identifier.
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
        public int? CategoryId { get; set; }

        /// <summary>
        /// the category this transaction belongs to.
        /// This is not set for transfer transactions.
        /// </summary>
        public CategoryEntity Category { get; set; }

        /// <summary>
        /// The identifier of the account this transaction belongs to.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The account this transaction belongs to.
        /// </summary>
        public AccountEntity Account { get; set; }

        /// <summary>
        /// The identifier of the account that is the receiver.
        /// Only has a value when this transaction is a transfer transaction.
        /// </summary>
        public int? ReceivingAccountId { get; set; }

        /// <summary>
        /// The account that is the receiver.
        /// Only has a value when this transaction is a transfer transaction.
        /// </summary>
        public AccountEntity ReceivingAccount { get; set; }

        /// <summary>
        /// A value indicating if this transaction needs to be manually confirmed before being processed.
        /// This can be useful when the exact date or amount is not known.
        /// </summary>
        public bool NeedsConfirmation { get; set; }

        /// <summary>
        /// The splits, containing all specifications of amounts in this transaction paid for others.
        /// This can only have entries when a <see cref="SplitwiseTransaction"/> is linked.
        /// </summary>
        public List<SplitDetailEntity> SplitDetails { get; set; }

        /// <summary>
        /// The collection of payment requests which are linked to this transaction.
        /// </summary>
        public List<PaymentRequestEntity> PaymentRequests { get; set; }
    }
}