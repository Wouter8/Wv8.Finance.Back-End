namespace PersonalFinance.Common.DataTransfer.Output
{
    /// <summary>
    /// A class for a data transfer object representing a split of a transaction.
    /// </summary>
    public class SplitDetail
    {
        /// <summary>
        /// The identifier of the transaction this split belongs to.
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// The identifier of the user from Splitwise who is linked to this split.
        /// </summary>
        public int SplitwiseUserId { get; set; }

        /// <summary>
        /// The amount split.
        /// </summary>
        public decimal Amount { get; set; }
    }
}