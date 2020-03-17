namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.History;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
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
                startDate: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1),
                endDate: LocalDate.FromDateTime(DateTime.Today).PlusDays(1));
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Expense - not to be processed
            this.context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = -20,
                    CategoryId = category.Id,
                    Date = LocalDate.FromDateTime(DateTime.Today).PlusDays(1),
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Expense,
                });
            this.context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(0, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(0, budget.Spent);

            // Expense - to be processed
            this.context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = -20,
                    CategoryId = category.Id,
                    Date = LocalDate.FromDateTime(DateTime.Today),
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Expense,
                });
            this.context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(-20, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);

            // Income - to be processed
            this.context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = 50,
                    CategoryId = category2.Id,
                    Date = LocalDate.FromDateTime(DateTime.Today),
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Income,
                });
            this.context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(30, account.CurrentBalance);
            Assert.Equal(0, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);

            // Transfer - to be processed
            this.context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = 30,
                    ReceivingAccountId = account2.Id,
                    Date = LocalDate.FromDateTime(DateTime.Today),
                    Description = "Description",
                    Processed = false,
                    Type = TransactionType.Transfer,
                });
            this.context.SaveChanges();

            this.TransactionProcessor.Run();

            budget = this.BudgetManager.GetBudget(budget.Id);
            account = this.AccountManager.GetAccount(account.Id);
            account2 = this.AccountManager.GetAccount(account2.Id);

            Assert.Equal(0, account.CurrentBalance);
            Assert.Equal(30, account2.CurrentBalance);
            Assert.Equal(20, budget.Spent);

            // Unconfirmed transaction - not to be processed
            this.context.Transactions.Add(
                new TransactionEntity
                {
                    AccountId = account.Id,
                    Amount = -30,
                    ReceivingAccountId = account2.Id,
                    Date = LocalDate.FromDateTime(DateTime.Today),
                    Description = "Description",
                    Processed = false,
                    CategoryId = category.Id,
                    Type = TransactionType.Expense,
                    NeedsConfirmation = true,
                    IsConfirmed = false,
                });
            this.context.SaveChanges();

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
            var account = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today); // 2 instances should be created, and finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = new RecurringTransactionEntity
            {
                Description = description,
                Type = TransactionType.Expense,
                Amount = amount,
                StartDate = startDate,
                EndDate = endDate,
                AccountId = account,
                CategoryId = category,
                ReceivingAccountId = null,
                Interval = interval,
                IntervalUnit = intervalUnit,
                NeedsConfirmation = false,
                NextOccurence = startDate,
            };
            this.context.RecurringTransactions.Add(rTransaction);
            this.context.SaveChanges();

            this.TransactionProcessor.Run();

            this.RefreshContext();

            rTransaction = this.context.RecurringTransactions.Single(rt => rt.Id == rTransaction.Id);
            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.True(rTransaction.Finished);
            Assert.Equal(2, instances.Count);
        }

        /// <summary>
        /// Tests that the historical balance is created upon creating an account.
        /// </summary>
        [Fact]
        public void HistoricalBalance_NewAccount()
        {
            var account = this.GenerateAccount();

            var historicBalances = this.context.AccountHistory.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(historicBalances);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(0, historicBalances.First().Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances.First().ValidTo);
        }

        /// <summary>
        /// Tests that the historical balance is properly stored upon processing a transaction.
        /// </summary>
        [Fact]
        public void HistoricalBalance_Transaction()
        {
            // All transaction should use alter the already existing historic entry, since it has the same date.
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Transaction that should be processed immediately.
            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: -50);
            var historicBalances = this.context.AccountHistory.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(historicBalances);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);

            // Income transaction that should be processed immediately.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Income,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: 50);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(historicBalances);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(0, historicBalances[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);

            // Transfer transaction that should be processed immediately.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Transfer,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: 50,
                receivingAccountId: account2.Id);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory.Where(ah => ah.AccountId == account.Id).ToList();
            var historicBalances2 = this.context.AccountHistory.Where(ah => ah.AccountId == account2.Id).ToList();

            Assert.Single(historicBalances);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);

            Assert.Single(historicBalances2);
            Assert.Single(historicBalances2.AtNow());
            Assert.Equal(50, historicBalances2[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);

            // Remove last transaction. Should add historical entry.
            this.TransactionManager.DeleteTransaction(transaction.Id);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory.Where(ah => ah.AccountId == account.Id).ToList();
            historicBalances2 = this.context.AccountHistory.Where(ah => ah.AccountId == account2.Id).ToList();

            Assert.Single(historicBalances);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(0, historicBalances[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);

            Assert.Single(historicBalances2);
            Assert.Single(historicBalances2.AtNow());
            Assert.Equal(0, historicBalances2[0].Balance);
            Assert.Equal(DateTime.MaxValue, historicBalances[0].ValidTo);
        }

        /// <summary>
        /// Tests that the historical balance is properly stored upon processing a transaction in the past.
        /// </summary>
        [Fact]
        public void HistoricalBalance_TransactionInPast()
        {
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Transaction that should be processed before account is created.
            // Should add a historical entry to the beginning.
            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-2),
                amount: -50);
            var historicBalances = this.context.AccountHistory
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();

            Assert.Equal(2, historicBalances.Count);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(-50, historicBalances[1].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[0].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances[1].ValidTo);

            // Transaction that should be processed between the already existing historical entries.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Income,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1),
                amount: 50);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();

            Assert.Equal(3, historicBalances.Count);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(0, historicBalances[1].Balance);
            Assert.Equal(0, historicBalances[2].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[0].ValidTo);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[1].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances[2].ValidTo);

            // Transfer transaction that should be processed at the same date as previous transaction.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Transfer,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1),
                amount: 50,
                receivingAccountId: account2.Id);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();
            var historicBalances2 = this.context.AccountHistory
                .Where(ah => ah.AccountId == account2.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();

            Assert.Equal(3, historicBalances.Count);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(-50, historicBalances[1].Balance);
            Assert.Equal(-50, historicBalances[2].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[0].ValidTo);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[1].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances[2].ValidTo);

            Assert.Equal(2, historicBalances2.Count);
            Assert.Single(historicBalances2.AtNow());
            Assert.Equal(50, historicBalances2[0].Balance);
            Assert.Equal(50, historicBalances2[1].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances2[0].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances2[1].ValidTo);

            // Remove last transaction. Should alter the latest historical entry.
            // Second account should have its first entry changed.
            this.TransactionManager.DeleteTransaction(transaction.Id);
            this.RefreshContext();
            historicBalances = this.context.AccountHistory
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();
            historicBalances2 = this.context.AccountHistory
                .Where(ah => ah.AccountId == account2.Id)
                .OrderBy(ah => ah.ValidFrom)
                .ToList();

            Assert.Equal(3, historicBalances.Count);
            Assert.Single(historicBalances.AtNow());
            Assert.Equal(-50, historicBalances[0].Balance);
            Assert.Equal(0, historicBalances[1].Balance);
            Assert.Equal(0, historicBalances[2].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[0].ValidTo);
            Assert.NotEqual(DateTime.MaxValue, historicBalances[1].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances[2].ValidTo);

            Assert.Equal(2, historicBalances2.Count);
            Assert.Single(historicBalances2.AtNow());
            Assert.Equal(0, historicBalances2[0].Balance);
            Assert.Equal(0, historicBalances2[1].Balance);
            Assert.NotEqual(DateTime.MaxValue, historicBalances2[0].ValidTo);
            Assert.Equal(DateTime.MaxValue, historicBalances2[1].ValidTo);
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