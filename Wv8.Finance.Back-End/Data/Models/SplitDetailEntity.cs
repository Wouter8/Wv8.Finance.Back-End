namespace PersonalFinance.Data.Models
{
    /// <summary>
    /// A class for entities which represent a split of a transaction managed in Splitwise.
    /// </summary>
    public class SplitDetailEntity
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