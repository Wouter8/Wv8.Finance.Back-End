namespace PersonalFinance.Data.External.Splitwise.Models
{
    /// <summary>
    /// An expense which was paid by someone else.
    /// </summary>
    public class OwedExpense : Expense
    {
        /// <summary>
        /// The part of the expense which is owed to the payer.
        /// </summary>
        public decimal AmountOwed { get; set; }
    }
}