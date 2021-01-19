namespace PersonalFinance.Data.Splitwise
{
    using System;
    using System.Collections.Generic;
    using DataTransfer;

    public interface ISplitwiseContext
    {
        public List<Expense> GetExpenses(DateTime updatedAfter);
    }
}