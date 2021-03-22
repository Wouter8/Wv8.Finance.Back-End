namespace PersonalFinance.Data.External.Splitwise.DataTransfer
{
    using System.Collections.Generic;

    /// <summary>
    /// A group in Splitwise.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// The members of the group.
        /// </summary>
        public List<User> Members { get; set; }
    }
}