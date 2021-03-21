namespace Business.UnitTest.Mocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NodaTime;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.External.Splitwise.Models;

    /// <summary>
    /// A mock for the Splitwise contexts.
    /// </summary>
    public class SplitwiseContextMock : ISplitwiseContext
    {
        /// <summary>
        /// The identifier used for the next transaction.
        /// </summary>
        private static int nextExpenseId = 0;

        /// <summary>
        /// If this is <c>true</c>, then an extra delay is added when calling method
        /// <see cref="ISplitwiseContext.GetExpenses"/>.
        /// </summary>
        public bool ExtraTimeWhenImporting { get; set; } = false;

        /// <summary>
        /// The list of mocked expenses.
        /// </summary>
        public List<Expense> Expenses { get; } = new List<Expense>();

        /// <summary>
        /// The list of mocked users.
        /// </summary>
        public List<User> Users { get; } = new List<User>();

        /// <inheritdoc />
        public Expense CreateExpense(decimal totalAmount, string description, LocalDate date, List<Split> splits)
        {
            var totalAmountPositive = Math.Abs(totalAmount);
            var personalAmount = totalAmountPositive - splits.Sum(s => s.Amount);
            var expense = new Expense
            {
                Id = ++nextExpenseId,
                Date = date,
                Description = description,
                PaidAmount = totalAmountPositive,
                PersonalAmount = personalAmount,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
                Splits = splits,
            };

            this.Expenses.Add(expense);

            return expense;
        }

        /// <inheritdoc />
        public void DeleteExpense(int id)
        {
            this.Expenses.RemoveAll(e => e.Id == id);
        }

        /// <inheritdoc />
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            if (this.ExtraTimeWhenImporting)
                Thread.Sleep(1000);

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