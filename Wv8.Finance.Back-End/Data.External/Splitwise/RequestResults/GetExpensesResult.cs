namespace Data.External.Splitwise.RequestResults
{
    using System.Collections.Generic;
    using PersonalFinance.Data.Splitwise.DataTransfer;

    public class GetExpensesResult
    {
        public List<Expense> Expenses { get; set; }
    }
}