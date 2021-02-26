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
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.Exceptions;
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
            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);
            Assert.Empty(transactions);
        }

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that the not-importable transactions are correctly filtered.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_ImportableFilter()
        {
            this.context.GenerateSplitwiseTransaction(1, imported: false, paidAmount: 0);
            this.context.GenerateSplitwiseTransaction(2, imported: true, paidAmount: 0); // Not importable
            this.context.GenerateSplitwiseTransaction(3, imported: false, paidAmount: 10m); // Not importable
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);
            Assert.Equal(3, transactions.Count);

            transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
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

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            Assert.Equal(2, transactions.First().Id);
            Assert.Equal(1, transactions.Skip(1).First().Id);
            Assert.Equal(3, transactions.Skip(2).First().Id);
        }

        #endregion GetSplitwiseTransactions

        #region ImportTransaction

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportTransaction"/>.
        /// Verifies that an exception is thrown if the Splitwise transaction contains a paid amount.
        /// </summary>
        [Fact]
        public void Test_ImportTransaction_PaidByMe()
        {
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                    1, date: DateTime.Today.ToLocalDate(), paidAmount: 10, personalAmount: 2.5M);

            this.context.SaveChanges();

            Assert.Throws<ValidationException>(() =>
                this.SplitwiseManager.ImportTransaction(splitwiseTransaction.Id, category.Id));
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportTransaction"/>.
        /// Verifies that a transaction is correctly imported when someone paid for me.
        /// </summary>
        [Fact]
        public void Test_ImportTransaction()
        {
            var (account, _) = this.context.GenerateAccount();
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1, date: DateTime.Today.ToLocalDate(), paidAmount: 0, personalAmount: 2.5M);

            this.context.SaveChanges();

            var transaction = this.SplitwiseManager.ImportTransaction(splitwiseTransaction.Id, category.Id);

            Assert.Equal(-splitwiseTransaction.PersonalAmount, transaction.PersonalAmount);
            Assert.Equal(-splitwiseTransaction.PaidAmount, transaction.Amount);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal(splitwiseAccount.Id, transaction.AccountId);
            Assert.Equal(splitwiseTransaction.Description, transaction.Description);
            Assert.Equal(splitwiseTransaction.Date.ToDateString(), transaction.Date);
            Assert.Equal(splitwiseTransaction.Id, transaction.SplitwiseTransactionId);
            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(TransactionType.Expense, transaction.Type);

            this.RefreshContext();

            var accountBalance = this.context.Accounts.GetEntity(account.Id).CurrentBalance;
            var splitwiseBalance = this.context.Accounts.GetEntity(splitwiseAccount.Id).CurrentBalance;

            Assert.Equal(0, accountBalance);
            Assert.Equal(-2.5M, splitwiseBalance);
        }

        #endregion ImportTransaction

        #region ImportFromSplitwise

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that all transactions are imported when there is no transaction imported yet from which the updated
        /// at time is filled.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_NoInitialDate()
        {
            // No transactions in database.
            this.SplitwiseContextMock.GenerateExpense(1, updatedAt: DateTime.MinValue.AddMilliseconds(1), paidAmount: 0);
            this.SplitwiseContextMock.GenerateExpense(2, updatedAt: DateTime.MaxValue, paidAmount: 0);
            this.SplitwiseContextMock.GenerateExpense(3, updatedAt: DateTime.Now, paidAmount: 0);

            this.SplitwiseManager.ImportFromSplitwise();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            // Verify that all transactions are imported.
            Assert.Equal(3, transactions.Count);
            Assert.Contains(transactions, t => t.Id == 1);
            Assert.Contains(transactions, t => t.Id == 2);
            Assert.Contains(transactions, t => t.Id == 3);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that only transactions after the latest known updated at value are imported.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_WithInitialDate()
        {
            // Add Splitwise transaction to database.
            this.context.GenerateSplitwiseTransaction(1, updatedAt: DateTime.Now.AddDays(-7));
            this.context.SaveChanges();

            this.SplitwiseContextMock.GenerateExpense(2, updatedAt: DateTime.MinValue, paidAmount: 0);
            this.SplitwiseContextMock.GenerateExpense(3, updatedAt: DateTime.MaxValue, paidAmount: 0);
            this.SplitwiseContextMock.GenerateExpense(4, updatedAt: DateTime.Now, paidAmount: 0);

            this.SplitwiseManager.ImportFromSplitwise();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            // Verify that all transactions which have a later updated at value are imported.
            Assert.Equal(3, transactions.Count);
            Assert.Contains(transactions, t => t.Id == 1);
            // Transaction with id 2 should not be imported.
            Assert.Contains(transactions, t => t.Id == 3);
            Assert.Contains(transactions, t => t.Id == 4);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions for which the user paid an amount are not imported.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_PaidNotImported()
        {
            // No transactions in database.
            this.SplitwiseContextMock.GenerateExpense(1, paidAmount: 10);

            this.SplitwiseManager.ImportFromSplitwise();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            // Verify that transaction is not imported.
            Assert.Empty(transactions);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions for which only other people paid are imported.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_OthersPaidImported()
        {
            // No transactions in database.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                paidAmount: 0,
                personalAmount: 10,
                description: "Description",
                date: new LocalDate(2021, 2, 19),
                isDeleted: false,
                updatedAt: new DateTime(2021, 2, 19, 10, 0, 0));

            this.SplitwiseManager.ImportFromSplitwise();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            // Verify that transaction is imported.
            var transaction = Assert.Single(transactions);
            Assert.Equal(expense.Id, transaction.Id);
            Assert.Equal(expense.PaidAmount, transaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, transaction.PersonalAmount);
            Assert.Equal(expense.Description, transaction.Description);
            Assert.Equal(expense.Date, transaction.Date);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions that are already known are properly updated.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Updated()
        {
            // Add Splitwise transaction to database.
            this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.Now.AddDays(-7),
                paidAmount: 0,
                personalAmount: 10,
                date: new LocalDate(2021, 02, 19));
            this.context.SaveChanges();

            // Add new version to mock.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                paidAmount: 0,
                personalAmount: 25,
                date: new LocalDate(2021, 02, 15));

            this.SplitwiseManager.ImportFromSplitwise();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);

            // Verify that transaction is updated.
            var transaction = Assert.Single(transactions);
            Assert.Equal(expense.Id, transaction.Id);
            Assert.Equal(expense.PaidAmount, transaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, transaction.PersonalAmount);
            Assert.Equal(expense.Description, transaction.Description);
            Assert.Equal(expense.Date, transaction.Date);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions that are already known are properly updated, even if they are already completely
        /// imported and processed.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_UpdatedAlreadyProcessed()
        {
            // Add processed Splitwise transaction to database.
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.Now.AddDays(-7),
                paidAmount: 0,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                imported: true);
            this.context.GenerateTransaction(
                splitwiseAccount,
                TransactionType.Expense,
                splitwiseTransaction.Description,
                splitwiseTransaction.Date,
                splitwiseTransaction.PaidAmount,
                category,
                splitwiseTransaction: splitwiseTransaction);
            this.SaveAndProcess();

            // Add new version to mock. In future so should not be processed.
            this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                paidAmount: 0,
                personalAmount: 15,
                date: DateTime.Today.AddDays(1).ToLocalDate(),
                description: "Description");
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(0, accountBalance);

            // Add new version to mock. Current date so should be processed.
            this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                paidAmount: 0,
                personalAmount: 15,
                date: DateTime.Today.ToLocalDate(),
                description: "Description");
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert
            this.RefreshContext();
            accountBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(-15, accountBalance);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that a known transaction is removed properly.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Removed()
        {
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.Now.AddDays(-7),
                paidAmount: 0,
                personalAmount: 10,
                date: DateTime.Today.AddDays(1).ToLocalDate(),
                description: "Description",
                imported: false);
            this.context.SaveChanges();

            // Add new version to mock. New version is removed.
            this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                isDeleted: true,
                paidAmount: splitwiseTransaction.PaidAmount,
                personalAmount: splitwiseTransaction.PersonalAmount,
                date: splitwiseTransaction.Date,
                description: splitwiseTransaction.Description);
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify removal.
            this.RefreshContext();
            splitwiseTransaction = this.context.SplitwiseTransactions.Single(t => t.Id == splitwiseTransaction.Id);
            Assert.True(splitwiseTransaction.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that a known transaction is removed properly.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_RemovedAlreadyProcessed()
        {
            var category = this.context.GenerateCategory();
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.Now.AddDays(-7),
                paidAmount: 0,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                imported: true);
            var transaction = this.context.GenerateTransaction(
                splitwiseAccount,
                TransactionType.Expense,
                splitwiseTransaction.Description,
                splitwiseTransaction.Date,
                splitwiseTransaction.PaidAmount,
                category,
                splitwiseTransaction: splitwiseTransaction);
            this.SaveAndProcess();

            // Add new version to mock. New version is removed.
            this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                isDeleted: true,
                paidAmount: splitwiseTransaction.PaidAmount,
                personalAmount: splitwiseTransaction.PersonalAmount,
                date: splitwiseTransaction.Date,
                description: splitwiseTransaction.Description);
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert and removal.
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(0, accountBalance);
            var transactionMaybe = this.context.Transactions.SingleOrNone(t => t.Id == transaction.Id);
            Assert.True(transactionMaybe.IsNone);
            splitwiseTransaction = this.context.SplitwiseTransactions.Single(t => t.Id == splitwiseTransaction.Id);
            Assert.True(splitwiseTransaction.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that no transaction is imported if the transaction is not known but is already removed in Splitwise.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_RemovedNotKnown()
        {
            // Add new transaction to mock. Transaction is already removed.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.Now,
                isDeleted: true,
                paidAmount: 0,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description");
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify removal.
            this.RefreshContext();
            var splitwiseTransaction = this.context.SplitwiseTransactions.Single(t => t.Id == expense.Id);
            Assert.True(splitwiseTransaction.IsDeleted);
        }

        #endregion ImportFromSplitwise

        #region GetSplitwiseUsers

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.GetSplitwiseUsers"/>.
        /// Verifies that the name is correctly set.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseUsers_Name()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", "van Acht");

            var users = this.SplitwiseManager.GetSplitwiseUsers();

            var user = users.Single();
            Assert.Equal(1, user.Id);
            Assert.Equal("Wouter van Acht", user.Name);
        }

        #endregion GetSplitwiseUsers
    }
}