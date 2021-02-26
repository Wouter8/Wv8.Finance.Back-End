﻿namespace PersonalFinance.Common.DataTransfer.Output
{
    using System.Collections.Generic;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// An entity representing a transaction. This can be an income, expense or transfer.
    /// </summary>
    public class Transaction
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
        /// The date this transaction occured. Note that this can be in the future.
        /// </summary>
        public string Date { get; set; }

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
        /// A value indicating if this transaction has been processed.
        /// This value will be false for transactions in the future, and true for transactions in the past.
        /// It can be either for transactions on the current date, depending if the process timer has already ticked.
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Optionally, the identifier of the recurring transaction this transaction descended from.
        /// </summary>
        public Maybe<int> RecurringTransactionId { get; set; }

        /// <summary>
        /// Optionally, the recurring transaction this transaction descended from.
        /// </summary>
        public Maybe<RecurringTransaction> RecurringTransaction { get; set; }

        /// <summary>
        /// A value indicating if this transaction needs to be manually confirmed before being processed.
        /// This can be useful when the exact date or amount is not known.
        /// </summary>
        public bool NeedsConfirmation { get; set; }

        /// <summary>
        /// A value indicating if this transaction is manually confirmed to be processed.
        /// Only filled if <see cref="NeedsConfirmation"/> is true.
        /// </summary>
        public Maybe<bool> IsConfirmed { get; set; }

        /// <summary>
        /// The collection of payment requests which are linked to this transaction.
        /// </summary>
        public List<PaymentRequest> PaymentRequests { get; set; }

        /// <summary>
        /// The personal amount (amount - payment requests) of the transaction.
        /// </summary>
        public decimal PersonalAmount { get; set; }

        /// <summary>
        /// Optionally, the identifier of the Splitwise transaction this transaction is linked to.
        /// </summary>
        public Maybe<int> SplitwiseTransactionId { get; set; }

        /// <summary>
        /// Optionally, the Splitwise transaction this transaction is linked to.
        /// </summary>
        public Maybe<SplitwiseTransaction> SplitwiseTransaction { get; set; }

        // TODO: Add Savings
    }
}