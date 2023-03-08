namespace PersonalFinance.Common.DataTransfer.Input
{
    /// <summary>
    /// A class to indicate a (partial) split to a user in Splitwise.
    /// </summary>
    public class InputSplitwiseSplit
    {
        /// <summary>
        /// The identifier of the Splitwise user the split is meant for.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// The amount paid for the user.
        /// </summary>
        public decimal Amount { get; set; }
    }
}
