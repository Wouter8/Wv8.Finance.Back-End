namespace Business.UnitTest.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Business.UnitTest.Integration.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// Tests for the reports manager.
    /// </summary>
    public class ReportTests : BaseIntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportTests"/> class.
        /// </summary>
        /// <param name="spFixture">See <see cref="BaseIntegrationTest"/>.</param>
        public ReportTests(ServiceProviderFixture spFixture)
            : base(spFixture)
        {
        }

        #region GetCurrentDateReport

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the net worth is properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_NetWorth()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();
            var account3 = this.GenerateAccount();

            // Add transactions to account and mark obsolete later
            this.GenerateTransaction(accountId: account3.Id, amount: -50, date: DateTime.Today.AddDays(-3).ToLocalDate());
            this.GenerateTransaction(
                accountId: account3.Id, type: TransactionType.Income, amount: 50, date: DateTime.Today.AddDays(-2).ToLocalDate());
            this.AccountManager.SetAccountObsolete(account3.Id, true);

            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-7).ToLocalDate()); // -50
            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-6).ToLocalDate()); // -100
            this.GenerateTransaction( // -50
                accountId: account1.Id, amount: 50, date: DateTime.Today.AddDays(-6).ToLocalDate());
            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-5).ToLocalDate()); // -100
            this.GenerateTransaction(
                accountId: account1.Id,
                type: TransactionType.Transfer,
                amount: 50,
                date: DateTime.Today.AddDays(-4).ToLocalDate(),
                receivingAccountId: account2.Id); // -100
            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-3).ToLocalDate()); // -150
            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-2).ToLocalDate()); // -200
            this.GenerateTransaction(accountId: account1.Id, amount: -50, date: DateTime.Today.AddDays(-1).ToLocalDate()); // -250
            this.GenerateTransaction(accountId: account1.Id, amount: -100, date: DateTime.Today.ToLocalDate()); // -350

            var report = this.ReportManager.GetCurrentDateReport();

            Assert.Equal(-350, report.NetWorth);
        }

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the accounts are properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_Accounts()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            this.AccountManager.SetAccountObsolete(account2.Id, true);

            var report = this.ReportManager.GetCurrentDateReport();

            Assert.Single(report.Accounts);
            Assert.Contains(report.Accounts, a => a.Id == account1.Id);
            Assert.DoesNotContain(report.Accounts, a => a.Id == account2.Id);
        }

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the historical balances are properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_HistoricalBalances()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();
            var account3 = this.GenerateAccount(); // Obsolete account, take historical balance into account.

            this.GenerateTransaction(
                accountId: account3.Id, date: DateTime.Today.AddDays(-7).ToLocalDate(), amount: -50);
            this.GenerateTransaction(
                accountId: account3.Id,
                date: DateTime.Today.AddDays(-3).ToLocalDate(),
                amount: 50);
            this.AccountManager.SetAccountObsolete(account3.Id, true);

            // Transaction for today
            this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.ToLocalDate(), amount: -50);
            // Transaction in the future - unprocessed
            this.GenerateTransaction(
                accountId: account1.Id, date: DateTime.Today.AddDays(7).ToLocalDate(), amount: -50);
            // Transaction in the past
            this.GenerateTransaction(
                accountId: account1.Id, date: DateTime.Today.AddDays(-7).ToLocalDate(), amount: -50);
            // Transaction in between
            this.GenerateTransaction(
                accountId: account1.Id, date: DateTime.Today.AddDays(-3).ToLocalDate(), amount: -50);
            this.GenerateTransaction(
                accountId: account2.Id, date: DateTime.Today.AddDays(-3).ToLocalDate(), amount: -50);
            // Transaction on already existing date, but for different accounts
            this.GenerateTransaction(
                accountId: account2.Id, date: DateTime.Today.AddDays(-7).ToLocalDate(), amount: -50);
            this.GenerateTransaction(
                accountId: account1.Id,
                type: TransactionType.Transfer,
                date: DateTime.Today.AddDays(-3).ToLocalDate(),
                amount: 50,
                receivingAccountId: account2.Id);

            var report = this.ReportManager.GetCurrentDateReport();
            var historicalEntries = report.HistoricalBalance.Values.ToList();

            var historicalEntriesVerification = new List<dynamic>
            {
                new
                {
                    StartDay = 0,
                    Value = 0,
                },
                new
                {
                    StartDay = 7,
                    Value = -150,
                },
                new
                {
                    StartDay = 11,
                    Value = -200,
                },
                new
                {
                    StartDay = 14,
                    Value = -250,
                },
            };
            historicalEntriesVerification.Reverse();

            Assert.Equal(21, report.HistoricalBalance.Count);
            for (var i = 0; i < historicalEntries.Count; i++)
            {
                var entry = historicalEntries[i];
                var verification = historicalEntriesVerification.First(e => i >= e.StartDay);

                Assert.Equal(verification.Value, entry);
            }

            Assert.True(report.HistoricalBalance.ContainsKey(DateTime.Today.AddDays(-7).ToLocalDate().ToDateString()));
            Assert.True(report.HistoricalBalance.ContainsKey(DateTime.Today.AddDays(-3).ToLocalDate().ToDateString()));
            Assert.True(report.HistoricalBalance.ContainsKey(DateTime.Today.ToLocalDate().ToDateString()));
        }

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the latest transactions are properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_LatestTransactions()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var latestTransactions = new List<int>
            {
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-3).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-2).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-1).ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    type: TransactionType.Transfer,
                    date: DateTime.Today.AddDays(-1).ToLocalDate(),
                    receivingAccountId: account2.Id).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.ToLocalDate()).Id,
            };
            var missingTransactions = new List<int>
            {
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-7).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-6).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-5).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-4).ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(-1).ToLocalDate(),
                    needsConfirmation: true).Id,
            };

            var report = this.ReportManager.GetCurrentDateReport();

            Assert.Equal(5, report.LatestTransactions.Count);
            Assert.True(report.LatestTransactions.All(t => latestTransactions.Contains(t.Id)));
            Assert.DoesNotContain(report.LatestTransactions, t => missingTransactions.Contains(t.Id));

            Assert.Equal(latestTransactions.Last(), report.LatestTransactions.First().Id);
            Assert.Equal(latestTransactions.First(), report.LatestTransactions.Last().Id);
        }

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the upcoming transactions are properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_UpcomingTransactions()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var upcomingTransactions = new List<int>
            {
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(1).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(2).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(3).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(4).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(5).ToLocalDate()).Id,
            };
            var missingTransactions = new List<int>
            {
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-3).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-2).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-1).ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    type: TransactionType.Transfer,
                    date: DateTime.Today.AddDays(-1).ToLocalDate(),
                    receivingAccountId: account2.Id).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(3).ToLocalDate(),
                    needsConfirmation: true).Id,
            };

            var report = this.ReportManager.GetCurrentDateReport();

            Assert.Equal(5, report.UpcomingTransactions.Count);
            Assert.True(report.UpcomingTransactions.All(t => upcomingTransactions.Contains(t.Id)));
            Assert.DoesNotContain(report.UpcomingTransactions, t => missingTransactions.Contains(t.Id));

            Assert.Equal(upcomingTransactions.First(), report.UpcomingTransactions.First().Id);
            Assert.Equal(upcomingTransactions.Last(), report.UpcomingTransactions.Last().Id);
        }

        /// <summary>
        /// Tests the <see cref="IReportManager.GetCurrentDateReport"/> method.
        /// Tests that the unconfirmed transactions are properly set.
        /// </summary>
        [Fact]
        public void GetCurrentDateReport_UnconfirmedTransactions()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var unconfirmedTransactions = new List<int>
            {
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(-3).ToLocalDate(),
                    needsConfirmation: true).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(-2).ToLocalDate(),
                    needsConfirmation: true).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(-1).ToLocalDate(),
                    needsConfirmation: true).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(1).ToLocalDate(),
                    needsConfirmation: true).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(2).ToLocalDate(),
                    needsConfirmation: true).Id,
            };
            var missingTransactions = new List<int>
            {
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-3).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-2).ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(-1).ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    type: TransactionType.Transfer,
                    date: DateTime.Today.AddDays(-1).ToLocalDate(),
                    receivingAccountId: account2.Id).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.ToLocalDate()).Id,
                this.GenerateTransaction(accountId: account1.Id, date: DateTime.Today.AddDays(1).ToLocalDate()).Id,
                this.GenerateTransaction(
                    accountId: account1.Id,
                    date: DateTime.Today.AddDays(3).ToLocalDate(),
                    needsConfirmation: true).Id,
            };

            var report = this.ReportManager.GetCurrentDateReport();

            Assert.Equal(5, report.UnconfirmedTransactions.Count);
            Assert.True(report.UnconfirmedTransactions.All(t => unconfirmedTransactions.Contains(t.Id)));
            Assert.DoesNotContain(report.UnconfirmedTransactions, t => missingTransactions.Contains(t.Id));

            Assert.Equal(unconfirmedTransactions.First(), report.UnconfirmedTransactions.First().Id);
            Assert.Equal(unconfirmedTransactions.Last(), report.UnconfirmedTransactions.Last().Id);
        }

        #endregion GetCurrentDateReport

        #region GetCategoryReport

        /// <summary>
        /// Tests whether the report is correctly calculated.
        /// </summary>
        [Fact]
        public void GetCategoryReport_Results()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();

            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 03);

            // Add relevant transactions
            // Day 1: -70 + 30 = -40
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 01), amount: -50);
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 01), amount: -20);
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 01), amount: 30);
            // Day 2: +50
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 02), amount: 50);
            // Day 3: - 20
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 03), amount: -20);

            this.context.SaveChanges();

            var report = this.ReportManager.GetCategoryReport(category.Id, start.ToDateString(), end.ToDateString());

            var results = new List<(decimal expenses, decimal incomes, decimal result)>
            {
                (-70, 30, -40),
                (0, 50, 50),
                (-20, 0, -20),
            };

            for (var i = 0; i < 3; i++)
            {
                var (expenses, incomes, result) = results[i];

                Assert.Equal(expenses, report.Expenses[i]);
                Assert.Equal(incomes, report.Incomes[i]);
                Assert.Equal(result, report.Results.Value[i]);
            }
        }

        /// <summary>
        /// Tests whether an empty report is returned when there's no data for the category.
        /// </summary>
        [Fact]
        public void GetCategoryReport_NoData()
        {
            var category = this.context.GenerateCategory();

            // 12 days
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 12);

            this.context.SaveChanges();

            var report = this.ReportManager.GetCategoryReport(category.Id, start.ToDateString(), end.ToDateString());

            Assert.All(report.Expenses, v => Assert.True(v == 0));
            Assert.All(report.Incomes, v => Assert.True(v == 0));
            Wv8Assert.IsNone(report.Results);
        }

        /// <summary>
        /// Tests whether the correct transactions are included in the report.
        /// </summary>
        [Fact]
        public void GetCategoryReport_TransactionFiltered_Date()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();

            // 12 days
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 12);

            // Add transactions outside of range
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2020, 01, 01));
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2020, 12, 31));
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 13));
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 12, 31));
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2020, 01, 01));
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2020, 12, 31));
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 13));
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 12, 31));

            this.context.SaveChanges();

            var report = this.ReportManager.GetCategoryReport(category.Id, start.ToDateString(), end.ToDateString());

            Assert.All(report.Expenses, v => Assert.True(v == 0));
            Assert.All(report.Incomes, v => Assert.True(v == 0));
            Wv8Assert.IsNone(report.Results);
        }

        /// <summary>
        /// Tests whether the correct transactions are included in the report.
        /// </summary>
        [Fact]
        public void GetCategoryReport_TransactionFiltered_DifferentCategory()
        {
            var category1 = this.context.GenerateCategory();
            var category2 = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();

            // 12 days
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 12);

            // Add transactions to category2
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category2, date: Ld(2021, 01, 01));

            this.context.SaveChanges();

            // Request report for category1
            var report = this.ReportManager.GetCategoryReport(category1.Id, start.ToDateString(), end.ToDateString());

            Assert.All(report.Expenses, v => Assert.True(v == 0));
            Assert.All(report.Incomes, v => Assert.True(v == 0));
            Wv8Assert.IsNone(report.Results);
        }

        /// <summary>
        /// Tests whether an exception is thrown if the category does not exist.
        /// </summary>
        [Fact]
        public void GetCategoryReport_NonExistingCategory()
        {
            var nonExistingId = -1;
            Wv8Assert.Throws<DoesNotExistException>(
                () => this.ReportManager.GetCategoryReport(
                    nonExistingId, Ld(2021, 01, 01).ToDateString(), Ld(2021, 01, 03).ToDateString()),
                $"Category with identifier {nonExistingId} does not exist.");
        }

        /// <summary>
        /// Tests whether the results are <c>None</c> when the category only contain expenses.
        /// </summary>
        [Fact]
        public void GetCategoryReport_OnlyExpenses()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();

            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 03);

            // Add expense transactions
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 01), amount: -50);
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 01), amount: -20);
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 02), amount: -50);
            this.context.GenerateTransaction(account, TransactionType.Expense, category: category, date: Ld(2021, 01, 03), amount: -20);

            this.context.SaveChanges();

            var report = this.ReportManager.GetCategoryReport(category.Id, start.ToDateString(), end.ToDateString());

            Assert.All(report.Incomes, i => Assert.Equal(0, i));
            Wv8Assert.IsNone(report.Results);
        }

        /// <summary>
        /// Tests whether the results are <c>None</c> when the category only contain income transactions.
        /// </summary>
        [Fact]
        public void GetCategoryReport_OnlyIncomes()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();

            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 03);

            // Add expense transactions
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 01), amount: 50);
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 01), amount: 20);
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 02), amount: 50);
            this.context.GenerateTransaction(account, TransactionType.Income, category: category, date: Ld(2021, 01, 03), amount: 20);

            this.context.SaveChanges();

            var report = this.ReportManager.GetCategoryReport(category.Id, start.ToDateString(), end.ToDateString());

            Assert.All(report.Expenses, i => Assert.Equal(0, i));
            Wv8Assert.IsNone(report.Results);
        }

        #endregion GetCategoryReport

        private static LocalDate Ld(int year, int month, int day)
        {
            return new LocalDate(year, month, day);
        }
    }
}
