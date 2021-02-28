namespace PersonalFinance.Data.External.Splitwise.Models
{
    using Wv8.Core;

    /// <summary>
    /// A class for objects representing the split amount for a user of a transaction.
    /// </summary>
    public class Split
    {
        /// <summary>
        /// The user identifier from Splitwise. If <c>None</c>, then the user identifier of this application will be used.
        /// </summary>
        public Maybe<int> UserId { get; set; }

        /// <summary>
        /// The amount split.
        /// </summary>
        public decimal Amount { get; set; }
    }
}