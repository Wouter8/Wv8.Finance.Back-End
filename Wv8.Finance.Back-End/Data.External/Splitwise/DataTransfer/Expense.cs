namespace PersonalFinance.Data.Splitwise.DataTransfer
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class Expense
    {
        public int Id { get; set; }

        public string Description { get; set; }

        [JsonProperty("date")]
        public string DateString { get; set; }

        [JsonProperty("deleted_at")]
        public string DeletedAtString { get; set; }

        public decimal Cost { get; set; }

        public List<UserOwed> Users { get; set; }
    }
}