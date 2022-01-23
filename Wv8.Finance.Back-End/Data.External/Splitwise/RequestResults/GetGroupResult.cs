namespace PersonalFinance.Data.External.Splitwise.RequestResults
{
    using PersonalFinance.Data.External.Splitwise.DataTransfer;

    /// <summary>
    /// A class for the result of the GetGroup API method.
    /// </summary>
    public class GetGroupResult
    {
        /// <summary>
        /// The retrieved group.
        /// </summary>
        public Group Group { get; set; }
    }
}