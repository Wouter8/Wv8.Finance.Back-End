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

        /// <summary>
        /// The list of mocked users.
        /// </summary>
        public List<User> Users { get; } = new List<User>();

        /// <inheritdoc />
        public Expense CreateExpense(string description, LocalDate date, List<Split> splits)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void DeleteExpense(int id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            return this.Expenses
                .Where(e => e.UpdatedAt > updatedAfter)
                .ToList();
        }

        /// <inheritdoc />
        public List<User> GetUsers()
        {
            return this.Users;
        }
    }
}