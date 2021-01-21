namespace Business.UnitTest.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.External.Splitwise.Models;

    /// <summary>
    /// A mock for the Splitwise contexts.
    /// </summary>
    public class SplitwiseContextMock : ISplitwiseContext
    {
        /// <summary>
        /// The list of mocked expenses.
        /// </summary>
        public List<Expense> Expenses { get; } = new List<Expense>();

        /// <inheritdoc />
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            return this.Expenses
                .Where(e => e.UpdatedAt > updatedAfter)
                .ToList();
        }
    }
}