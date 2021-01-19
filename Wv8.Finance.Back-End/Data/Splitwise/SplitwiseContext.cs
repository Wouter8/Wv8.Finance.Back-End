namespace PersonalFinance.Data.Splitwise
{
    using System;
    using System.Collections.Generic;
    using PersonalFinance.Data.Splitwise.DataTransfer;

    public class SplitwiseContext : ISplitwiseContext
    {
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            var updatedAfterString = updatedAfter.ToString("O");
            var limit = 0; // 0 is unlimited.

            return null;
        }
    }
}