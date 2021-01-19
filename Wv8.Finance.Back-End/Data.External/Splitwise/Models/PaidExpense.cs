namespace PersonalFinance.Data.External.Splitwise.Models
{
    /// <summary>
    /// An expense which I paid for.
    /// </summary>
    public class PaidExpense : Expense
    {
        /// <summary>
        /// The total amount paid.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// The personal part of the expense.
        /// </summary>
        public decimal PersonalAmount { get; set; }

        /// <summary>
        /// The amount that is owed by others.
        /// This should be equal to <see cref="PaidAmount"/> minus <see cref="OwedAmount"/>.
        /// </summary>
        public decimal OwedAmount { get; set; }
    }
}