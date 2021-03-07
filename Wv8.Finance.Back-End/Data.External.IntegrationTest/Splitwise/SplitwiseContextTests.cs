namespace Data.External.IntegrationTest.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NodaTime;
    using PersonalFinance.Common;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.External.Splitwise.Models;
    using Wv8.Core.Collections;
    using Xunit;

    /// <summary>
    /// A test class for the <see cref="SplitwiseContext"/>.
    /// </summary>
    /// <remarks>These tests should be ran manually if any logic for communicating with Splitwise is changed.</remarks>
    public class SplitwiseContextTests : BaseTest
    {
        /// <summary>
        /// The user id of a different user than the user of the tests.
        /// </summary>
        private int otherUserId1 = 37525658;

        /// <summary>
        /// The user id of a different user than the user of the tests.
        /// </summary>
        private int otherUserId2 = 37525670;

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that an expense gets created correctly when multiple splits are provided.
        /// </summary>
        [Fact]
        public void Test_CreateExpense_MultiSplit()
        {
            var totalAmount = 100;
            var date = new LocalDate(2021, 3, 7);
            var description = "Split expense";
            var splits = new List<Split>
            {
                new Split { UserId = this.otherUserId1, Amount = 30 },
                new Split { UserId = this.otherUserId2, Amount = 50 },
            };

            var expense = this.splitwiseContext.CreateExpense(totalAmount, description, date, splits);

            Assert.Equal(20, expense.PersonalAmount);
            Assert.Equal(totalAmount, expense.PaidAmount);
            Assert.Equal(date, expense.Date);
            Assert.Equal(description, expense.Description);
            Assert.False(expense.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that an expense gets created correctly when a single split is provided.
        /// </summary>
        [Fact]
        public void Test_CreateExpense_SingleSplit()
        {
            var totalAmount = 100;
            var date = new LocalDate(2021, 3, 7);
            var description = "Split expense";
            var splits = new List<Split>
            {
                new Split { UserId = this.otherUserId1, Amount = 50 },
            };

            var expense = this.splitwiseContext.CreateExpense(totalAmount, description, date, splits);

            Assert.Equal(50, expense.PersonalAmount);
            Assert.Equal(totalAmount, expense.PaidAmount);
            Assert.Equal(date, expense.Date);
            Assert.Equal(description, expense.Description);
            Assert.False(expense.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that an expense gets created correctly when the total amount is split to other users.
        /// </summary>
        [Fact]
        public void Test_CreateExpense_CompleteSplit()
        {
            var totalAmount = 100;
            var date = new LocalDate(2021, 3, 7);
            var description = "Split expense";
            var splits = new List<Split>
            {
                new Split { UserId = this.otherUserId1, Amount = 50 },
                new Split { UserId = this.otherUserId2, Amount = 50 },
            };

            var expense = this.splitwiseContext.CreateExpense(totalAmount, description, date, splits);

            Assert.Equal(0, expense.PersonalAmount);
            Assert.Equal(totalAmount, expense.PaidAmount);
            Assert.Equal(date, expense.Date);
            Assert.Equal(description, expense.Description);
            Assert.False(expense.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that an expense gets correctly deleted.
        /// </summary>
        [Fact]
        public void Test_DeleteExpense()
        {
            var now = DateTime.Now;
            var expense = this.splitwiseContext.CreateExpense(
                10,
                "Description",
                new LocalDate(2021, 3, 7),
                new Split { UserId = this.otherUserId1, Amount = 50 }.Singleton());

            this.splitwiseContext.DeleteExpense(expense.Id);

            var expenses = this.splitwiseContext.GetExpenses(now);
            expense = expenses.Single(e => e.Id == expense.Id);
            Assert.True(expense.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that the correct expenses are retrieved.
        /// </summary>
        [Fact]
        public void Test_GetExpenses()
        {
            var oldExpense = this.splitwiseContext.CreateExpense(
                10,
                "Description",
                new LocalDate(2021, 3, 7),
                new Split { UserId = this.otherUserId1, Amount = 50 }.Singleton());

            var now = DateTime.Now;

            // Wait to make sure the updatedAfter is actually after "now".
            Thread.Sleep(100);

            var newExpense = this.splitwiseContext.CreateExpense(
                10,
                "Description",
                new LocalDate(2021, 3, 7),
                new Split { UserId = this.otherUserId1, Amount = 50 }.Singleton());

            var expenses = this.splitwiseContext.GetExpenses(now);

            var expense = Assert.Single(expenses);

            Assert.Equal(newExpense.Id, expense.Id);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseContext.CreateExpense"/>.
        /// Verifies that the correct users are retrieved.
        /// </summary>
        [Fact]
        public void Test_GetUsers()
        {
            var users = this.splitwiseContext.GetUsers();

            Assert.Equal(2, users.Count);
            Assert.Contains(users, u => u.Id == this.otherUserId1);
            Assert.Contains(users, u => u.Id == this.otherUserId2);
            Assert.DoesNotContain(users, u => u.Id == this.settings.Value.SplitwiseUserId);

            var otherUser1 = users.Single(u => u.Id == this.otherUserId1);
            var otherUser2 = users.Single(u => u.Id == this.otherUserId2);

            Assert.Equal("Wouter2", otherUser1.FirstName);
            Assert.Equal("Wouter3", otherUser2.FirstName);
        }
    }
}