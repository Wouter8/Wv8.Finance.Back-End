namespace PersonalFinance.Data.External.Splitwise.DataTransfer
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// An expense from Splitwise.
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// The identifier of the expense.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The description of the expense.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date, formatted as an ISO string, of the expense.
        /// </summary>
        [JsonProperty("date")]
        public string DateString { get; set; }

        /// <summary>
        /// The timestamp at which the expense was updated, formatted as an ISO string.
        /// </summary>
        [JsonProperty("updated_at")]
        public string UpdatedAtString { get; set; }

        /// <summary>
        /// The timestamp at which the expense was deleted, formatted as an ISO string.
        /// If the expense is not deleted, this is <c>null</c>.
        /// </summary>
        [JsonProperty("deleted_at")]
        public string DeletedAtString { get; set; }

        /// <summary>
        /// The total cost of this expense.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// The list of users involved in this expense, together with how much they paid/owed.
        /// </summary>
        public List<UserOwed> Users { get; set; }
    }
}
