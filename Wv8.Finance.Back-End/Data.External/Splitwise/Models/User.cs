namespace PersonalFinance.Data.External.Splitwise.Models
{
    using Wv8.Core;

    /// <summary>
    /// A user which is imported from Splitwise.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        /// <remarks>This identifier is directly imported from Splitwise.</remarks>
        public int Id { get; set; }

        /// <summary>
        /// The first name of the user.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user.
        /// </summary>
        public Maybe<string> LastName { get; set; }
    }
}