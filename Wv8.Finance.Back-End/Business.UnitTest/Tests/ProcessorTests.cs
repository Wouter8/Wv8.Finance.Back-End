namespace Business.UnitTest.Tests
{
    using System;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of <see cref="ITransactionProcessor"/>.
    /// </summary>
    public class ProcessorTests : BaseTest
    {
        /// <summary>
        /// Tests that transactions in the past get properly processed.
        /// </summary>
        [Fact]
        public void Transactions()
        {
            var category = this.GenerateCategory();
            var category2 = this.GenerateCategory(CategoryType.Income);
            var budget = this.GenerateBudget(
                categoryId: category.Id,
                startDate: DateTime.Today.AddDays(-1),
                endDate: DateTime.Today.AddDays(1));
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Expense - not to be processed
            this.Context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = -20,
                    CategoryId = category.Id,
                    Date = DateTime.Today.AddDays(1),
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Expense,
                });
            this.Context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(0, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(0, budget.Spent);

            // Expense - to be processed
            this.Context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = -20,
                    CategoryId = category.Id,
                    Date = DateTime.Today,
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Expense,
                });
            this.Context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(-20, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);

            // Income - to be processed
            this.Context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = 50,
                    CategoryId = category2.Id,
                    Date = DateTime.Today,
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Income,
                });
            this.Context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(30, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);

            // Transfer - to be processed
            this.Context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = 30,
                    ReceivingAccountId = account2.Id,
                    Date = DateTime.Today,
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Transfer,
                });
            this.Context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(0, account.CurrentBalance);
            Assert.Equal(30, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);
        }

        /// <summary>
        /// Tests that recurring transactions get properly processed.
        /// </summary>
        [Fact]
        public void RecurringTransactions()
        {
            // TODO: Implement
        }

        /// <summary>
        /// Tests that recurring budgets get properly processed.
        /// </summary>
        [Fact]
        public void RecurringBudgets()
        {
            // TODO: Implement
        }
    }
}