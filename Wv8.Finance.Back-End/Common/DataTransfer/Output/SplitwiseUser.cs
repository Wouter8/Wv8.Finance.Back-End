namespace PersonalFinance.Common.DataTransfer.Output
{
    /// <summary>
    /// A user which is imported from Splitwise.
    /// </summary>
    public class SplitwiseUser
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        /// <remarks>This identifier is directly imported from Splitwise.</remarks>
        public long Id { get; set; }

        /// <summary>
        /// The name of the user.
        /// </summary>
        public string Name { get; set; }
    }
}
