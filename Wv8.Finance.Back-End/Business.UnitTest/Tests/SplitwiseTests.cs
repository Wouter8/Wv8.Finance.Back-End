namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using Business.UnitTest.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Extensions;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="SplitwiseManager"/>.
    /// </summary>
    public class SplitwiseTests : BaseTest
    {
        #region GetSplitwiseTransactions

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that an empty list is returned if the database is empty.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_Empty()
        {
            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
            Assert.Empty(transactions);
        }

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that the imported transactions are correctly filtered.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_ImportedFilter()
        {
            this.context.GenerateSplitwiseTransaction(1, imported: false);
            this.context.GenerateSplitwiseTransaction(2, imported: true);
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
            Assert.Equal(2, transactions.Count);

            transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);
            Assert.Single(transactions);
            Assert.Equal(1, transactions.Single().Id);
        }

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that the returned transactions are correctly ordered on date.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_Ordering()
        {
            this.context.GenerateSplitwiseTransaction(1, date: new LocalDate(2021, 1, 5));
            this.context.GenerateSplitwiseTransaction(2, date: new LocalDate(2021, 1, 4));
            this.context.GenerateSplitwiseTransaction(3, date: new LocalDate(2021, 1, 6));
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);

            Assert.Equal(2, transactions.First().Id);
            Assert.Equal(1, transactions.Skip(1).First().Id);
            Assert.Equal(3, transactions.Skip(2).First().Id);
        }

        #endregion GetSplitwiseTransactions

        #region ImportTransaction

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportTransaction"/>.
        /// Verifies that a transaction is correctly imported when I paid for it.
        /// </summary>
        [Fact]
        public void Test_ImportTransaction_PaidByMe()
        {
            var account = this.context.GenerateAccount(1);
            var splitwiseAccount = this.context.GenerateAccount(2, AccountType.Splitwise);
            var category = this.context.GenerateCategory(1);

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                    1, date: DateTime.Today.ToLocalDate(), paidAmount: 10, personalAmount: 2.5M);

            this.context.SaveChanges();

            var transaction = this.SplitwiseManager.ImportTransaction(splitwiseTransaction.Id, account.Id, category.Id);

            Assert.Equal(splitwiseTransaction.PersonalAmount, transaction.PersonalAmount);
            Assert.Equal(splitwiseTransaction.PaidAmount, transaction.Amount);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal(account.Id, transaction.AccountId);
            Assert.Equal(splitwiseTransaction.Description, transaction.Description);
            Assert.Equal(splitwiseTransaction.Date.ToDateString(), transaction.Date);
            Assert.Equal(splitwiseTransaction.Id, transaction.SplitwiseTransactionId);
            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(TransactionType.Expense, transaction.Type);

            this.RefreshContext();

            var accountBalance = this.context.Accounts.GetEntity(account.Id).DailyBalances.OrderBy(db => db.Date).Last().Balance;
            var splitwiseBalance = this.context.Accounts.GetEntity(splitwiseAccount.Id).DailyBalances.OrderBy(db => db.Date).Last().Balance;

            Assert.Equal(-10, accountBalance);
            Assert.Equal(5, splitwiseBalance);
        }

        #endregion ImportTransaction
    }
}