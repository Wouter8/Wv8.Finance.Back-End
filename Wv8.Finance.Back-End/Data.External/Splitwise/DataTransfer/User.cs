namespace PersonalFinance.Data.External.Splitwise.DataTransfer
{
    using Newtonsoft.Json;

    /// <summary>
    /// A user from Splitwise.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The first name of the user.
        /// </summary>
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user.
        /// </summary>
        [JsonProperty("last_name")]
        public string LastName { get; set; }
    }
}
