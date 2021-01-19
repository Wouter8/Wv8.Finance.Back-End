namespace PersonalFinance.Data.External.Splitwise.Models
{
    using NodaTime;
    using Wv8.Core;

    /// <summary>
    /// A transaction from Splitwise.
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// The identifier of the expense.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of the expense.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date of the expense.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// <c>true</c> if the expense has been deleted, <c>false</c> otherwise.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}