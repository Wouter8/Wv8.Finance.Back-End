namespace PersonalFinance.Common.DataTransfer
{
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// An entity representing a blue print for a transaction which get created based on an interval.
    /// </summary>
    public class RecurringTransaction
    {
        /// <summary>
        /// The identifier of the transaction to be created.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of the transaction to be created.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date from which transactions should be created.
        /// If this is a date in the past, transactions will be created retroactively.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// The date till which transactions should be created. A (final) transaction can be created on this date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// The type of the transaction to be created.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// The amount of the transaction to be created.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The identifier of the category the transaction to be created belongs to.
        /// This value is not set for transfer transactions.
        /// </summary>
        public Maybe<int> CategoryId { get; set; }

        /// <summary>
        /// the category this transaction belongs to.
        /// This is not set for transfer transactions.
        /// </summary>
        public Maybe<Category> Category { get; set; }

        /// <summary>
        /// The identifier of the account the transaction to be created belongs to.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The account the transaction to be created belongs to.
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
        /// The date for the next transaction that will be created from this blueprint.
        /// </summary>
        public Maybe<string> NextOccurence { get; set; }

        /// <summary>
        /// A value indicating if the end date has passed. No more transactions will be created.
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// The unit in: '<see cref="Interval"/> units.
        /// </summary>
        public IntervalUnit IntervalUnit { get; set; }

        /// <summary>
        /// The x in: 'x <see cref="IntervalUnit"/>.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// A value indicating if transactions created by this blueprint need to be manually confirmed before being processed.
        /// This can be useful when the exact date or amount is not known.
        /// </summary>
        public bool NeedsConfirmation { get; set; }

        // TODO: Add Savings
    }
}