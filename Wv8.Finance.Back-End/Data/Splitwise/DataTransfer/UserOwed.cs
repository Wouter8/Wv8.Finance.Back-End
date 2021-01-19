namespace PersonalFinance.Data.Splitwise.DataTransfer
{
    using Newtonsoft.Json;

    public class UserOwed
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        public decimal PaidShare { get; set; }

        public decimal OwedShare { get; set; }
    }
}