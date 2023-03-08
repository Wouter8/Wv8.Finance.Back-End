namespace PersonalFinance.Data.External.Splitwise.Models
{
    /// <summary>
    /// A class for objects representing the split amount for a user of a transaction.
    /// </summary>
    public class Split
    {
        /// <summary>
        /// The user identifier from Splitwise.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// The name of the user from Splitwise.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The amount split.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
