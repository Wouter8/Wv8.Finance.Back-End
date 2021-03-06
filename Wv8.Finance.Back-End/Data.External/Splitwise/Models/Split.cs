namespace PersonalFinance.Data.External.Splitwise.Models
{
    using Wv8.Core;

    /// <summary>
    /// A class for objects representing the split amount for a user of a transaction.
    /// </summary>
    public class Split
    {
        /// <summary>
        /// The user identifier from Splitwise.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The amount split.
        /// </summary>
        public decimal Amount { get; set; }
    }
}