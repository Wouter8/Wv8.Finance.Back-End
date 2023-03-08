namespace PersonalFinance.Data.Models
{
    /// <summary>
    /// A class for entities which represent a split of a transaction managed in Splitwise.
    /// </summary>
    public class SplitDetailEntity
    {
        /// <summary>
        /// The identifier of this split detail.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The identifier of the Splitwise transaction this split belongs to.
        /// This might not have a value when the transaction is not yet processed and therefore no Splitwise
        /// transaction is created.
        /// </summary>
        public long? SplitwiseTransactionId { get; set; }

        /// <summary>
        /// The identifier of the transaction this split belongs to.
        /// This might not have a value when an expense is imported from Splitwise, but is not yet completely imported
        /// into a transaction in this application.
        /// </summary>
        public int? TransactionId { get; set; }

        /// <summary>
        /// The identifier of the user from Splitwise who is linked to this split.
        /// </summary>
        public long SplitwiseUserId { get; set; }

        /// <summary>
        /// The name of the user from Splitwise. This is used as back-up when the user has become obsolete.
        /// </summary>
        public string SplitwiseUserName { get; set; }

        /// <summary>
        /// The amount split.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
