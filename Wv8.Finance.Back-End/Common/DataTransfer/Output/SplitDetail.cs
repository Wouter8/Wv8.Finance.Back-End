namespace PersonalFinance.Common.DataTransfer.Output
{
    using Wv8.Core;

    /// <summary>
    /// A class for a data transfer object representing a split of a transaction.
    /// </summary>
    public class SplitDetail
    {
        /// <summary>
        /// The identifier of the transaction this split belongs to.
        /// </summary>
        public Maybe<int> TransactionId { get; set; }

        /// <summary>
        /// The identifier of the Splitwise transaction this split belongs to.
        /// </summary>
        public Maybe<long> SplitwiseTransactionId { get; set; }

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
