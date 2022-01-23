namespace PersonalFinance.Data.External.Splitwise.RequestResults
{
    using System.Collections.Generic;
    using PersonalFinance.Data.External.Splitwise.DataTransfer;

    /// <summary>
    /// A class for the result of the CreateExpense API method.
    /// </summary>
    public class CreateExpenseResult
    {
        /// <summary>
        /// The created expenses.
        /// </summary>
        public List<Expense> Expenses { get; set; }
    }
}