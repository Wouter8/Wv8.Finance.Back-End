namespace PersonalFinance.Data.External.Splitwise.DataTransfer
{
    using Newtonsoft.Json;

    /// <summary>
    /// A class containing the information about a user who took part in an expense.
    /// </summary>
    public class UserOwed
    {
        /// <summary>
        /// The identifier of the user.
        /// </summary>
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// The amount that the user has paid.
        /// </summary>
        [JsonProperty("paid_share")]
        public decimal PaidShare { get; set; }

        /// <summary>
        /// The amount that the user is owed.
        /// </summary>
        [JsonProperty("owed_share")]
        public decimal OwedShare { get; set; }
    }
}