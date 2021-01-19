namespace PersonalFinance.Data.External.Splitwise.RequestResults
{
    using System.Collections.Generic;
    using PersonalFinance.Data.External.Splitwise.DataTransfer;

    /// <summary>
    /// A class for the result of the GetExpenses API method from Splitwise.
    /// </summary>
    public class GetExpensesResult
    {
        /// <summary>
        /// The list of expenses.
        /// </summary>
        public List<Expense> Expenses { get; set; }
    }
}