namespace PersonalFinance.Data.Splitwise.DataTransfer
{
    using Newtonsoft.Json;

    public class UserOwed
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("paid_share")]
        public decimal PaidShare { get; set; }

        [JsonProperty("owed_share")]
        public decimal OwedShare { get; set; }
    }
}