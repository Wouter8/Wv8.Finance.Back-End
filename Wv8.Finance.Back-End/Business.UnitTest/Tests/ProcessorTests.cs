namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common;
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
            var category2 = this.GenerateCategory();
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
                    Type = TransactionType.Internal,
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
                    NeedsConfirmation = true,
                    IsConfirmed = false,
                    Type = TransactionType.Expense,
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
                Type = TransactionType.Expense,
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
        /// Tests that transaction instances in the future get created, but not processed.
        /// </summary>
        [Fact]
        public void RecurringTransactions_Future()
        {
            var account = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = LocalDate.FromDateTime(DateTime.Today);
            var endDate = LocalDate.FromDateTime(DateTime.Today.AddDays(14)); // 8 instances should be created
            var interval = 1;
            var intervalUnit = IntervalUnit.Days;

            var rTransaction = new RecurringTransactionEntity
            {
                Description = description,
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
                Type = TransactionType.Expense,
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

            Assert.False(rTransaction.Finished);
            Assert.Equal(8, instances.Count);
            Assert.Equal(1, instances.Count(t => t.Processed));
            Assert.Equal(7, instances.Count(t => !t.Processed));
        }

        /// <summary>
        /// Tests that a historical entry is added on a date in the past, while the last entry is
        /// even further in the past.
        /// </summary>
        [Fact]
        public void DailyBalance_OldLastEntry()
        {
            var account = this.GenerateAccount();

            var historicBalance = this.context.DailyBalances.Single();

            this.context.Remove(historicBalance);
            this.context.DailyBalances.Add(new DailyBalanceEntity
            {
                Date = DateTime.Today.AddDays(-7).ToLocalDate(),
                AccountId = account.Id,
                Balance = 0,
            });
            this.context.SaveChanges();

            var date = LocalDate.FromDateTime(DateTime.Today).PlusDays(-3);
            // Transaction in the past.
            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: date,
                amount: -50);

            this.RefreshContext();

            var dailyBalances = this.context.DailyBalances.OrderBy(ah => ah.Date).ToList();

            Assert.Equal(2, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == date));
            Assert.Equal(0, dailyBalances[0].Balance);
            Assert.Equal(-50, dailyBalances[1].Balance);
            Assert.Equal(DateTime.Today.AddDays(-3).ToLocalDate(), dailyBalances[1].Date);
        }

        /// <summary>
        /// Tests that the historical balance is created upon creating an account.
        /// </summary>
        [Fact]
        public void DailyBalance_NewAccount()
        {
            var account = this.GenerateAccount();

            var dailyBalances = this.context.DailyBalances.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(dailyBalances);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(0, dailyBalances.First().Balance);
        }

        /// <summary>
        /// Tests that the historical balance is properly stored upon processing a transaction.
        /// </summary>
        [Fact]
        public void DailyBalance_Transaction()
        {
            // All transaction should use alter the already existing historic entry, since it has the same date.
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Transaction that should be processed immediately.
            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: -50);
            var dailyBalances = this.context.DailyBalances.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(dailyBalances);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);

            // Income transaction that should be processed immediately.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: 50);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances.Where(ah => ah.AccountId == account.Id).ToList();

            Assert.Single(dailyBalances);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(0, dailyBalances[0].Balance);

            // Transfer transaction that should be processed immediately.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Internal,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: 50,
                receivingAccountId: account2.Id);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances.Where(ah => ah.AccountId == account.Id).ToList();
            var dailyBalances2 = this.context.DailyBalances.Where(ah => ah.AccountId == account2.Id).ToList();

            Assert.Single(dailyBalances);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);

            Assert.Single(dailyBalances2);
            Assert.Single(dailyBalances2.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(50, dailyBalances2[0].Balance);

            // Remove last transaction. Should add historical entry.
            this.TransactionManager.DeleteTransaction(transaction.Id);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances.Where(ah => ah.AccountId == account.Id).ToList();
            dailyBalances2 = this.context.DailyBalances.Where(ah => ah.AccountId == account2.Id).ToList();

            Assert.Single(dailyBalances);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(0, dailyBalances[0].Balance);

            Assert.Single(dailyBalances2);
            Assert.Single(dailyBalances2.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(0, dailyBalances2[0].Balance);
        }

        /// <summary>
        /// Tests that the historical balance is properly stored upon processing a transaction in the past.
        /// </summary>
        [Fact]
        public void DailyBalance_TransactionInPast()
        {
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Transaction that should be processed before account is created.
            // Should add a historical entry to the beginning.
            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-2),
                amount: -50);
            var dailyBalances = this.context.DailyBalances
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.Date)
                .ToList();

            Assert.Equal(2, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);
            Assert.Equal(-50, dailyBalances[1].Balance);

            // Transaction that should be processed between the already existing historical entries.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1),
                amount: 50);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.Date)
                .ToList();

            Assert.Equal(3, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);
            Assert.Equal(0, dailyBalances[1].Balance);
            Assert.Equal(0, dailyBalances[2].Balance);

            // Transfer transaction that should be processed at the same date as previous transaction.
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Internal,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1),
                amount: 50,
                receivingAccountId: account2.Id);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.Date)
                .ToList();
            var dailyBalances2 = this.context.DailyBalances
                .Where(ah => ah.AccountId == account2.Id)
                .OrderBy(ah => ah.Date)
                .ToList();

            Assert.Equal(3, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);
            Assert.Equal(-50, dailyBalances[1].Balance);
            Assert.Equal(-50, dailyBalances[2].Balance);

            Assert.Equal(2, dailyBalances2.Count);
            Assert.Single(dailyBalances2.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(50, dailyBalances2[0].Balance);
            Assert.Equal(50, dailyBalances2[1].Balance);

            // Remove last transaction. Should alter the latest historical entry.
            // Second account should have its first entry changed.
            this.TransactionManager.DeleteTransaction(transaction.Id);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.Date)
                .ToList();
            dailyBalances2 = this.context.DailyBalances
                .Where(ah => ah.AccountId == account2.Id)
                .OrderBy(ah => ah.Date)
                .ToList();

            Assert.Equal(3, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);
            Assert.Equal(0, dailyBalances[1].Balance);
            Assert.Equal(0, dailyBalances[2].Balance);

            Assert.Equal(2, dailyBalances2.Count);
            Assert.Single(dailyBalances2.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(0, dailyBalances2[0].Balance);
            Assert.Equal(0, dailyBalances2[1].Balance);

            // Transaction before first transaction
            transaction = this.GenerateTransaction(
                accountId: account.Id,
                date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-4),
                amount: -50);
            this.RefreshContext();
            dailyBalances = this.context.DailyBalances
                .Where(ah => ah.AccountId == account.Id)
                .OrderBy(ah => ah.Date)
                .ToList();

            Assert.Equal(4, dailyBalances.Count);
            Assert.Single(dailyBalances.Where(hb => hb.Date == DateTime.Today.ToLocalDate()));
            Assert.Equal(-50, dailyBalances[0].Balance);
            Assert.Equal(-100, dailyBalances[1].Balance);
            Assert.Equal(-50, dailyBalances[2].Balance);
            Assert.Equal(-50, dailyBalances[3].Balance);
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