namespace Business.UnitTest.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// Tests for the reports manager.
    /// </summary>
    public class ReportTests : BaseTest
    {
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
                accountId: account1.Id, type: TransactionType.Income, amount: 50, date: DateTime.Today.AddDays(-6).ToLocalDate());
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
                type: TransactionType.Income,
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

            Assert.Equal(3, report.HistoricalBalance.Count);
            Assert.Equal(-150, historicalEntries[0]);
            Assert.Equal(-150, historicalEntries[1]);
            Assert.Equal(-200, historicalEntries[2]);

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
    }
}
