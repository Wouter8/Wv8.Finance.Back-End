namespace Business.UnitTest.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Business.UnitTest.Integration.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise.Models;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="SplitwiseManager"/>.
    /// </summary>
    public class SplitwiseTests : BaseIntegrationTest
    {
        private int splitwiseUserId = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitwiseTests"/> class.
        /// </summary>
        /// <param name="spFixture">See <see cref="BaseIntegrationTest"/>.</param>
        public SplitwiseTests(ServiceProviderFixture spFixture)
            : base(spFixture)
        {
        }

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
            this.context.GenerateSplitwiseTransaction(2, imported: false, paidAmount: 10m);
            this.context.GenerateSplitwiseTransaction(3, imported: true, paidAmount: 0); // Not importable
            this.context.GenerateSplitwiseTransaction(4, imported: false, paidAmount: 10m, isDeleted: true); // Not importable
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);
            Assert.Equal(4, transactions.Count);

            transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
            Assert.Equal(2, transactions.Count);

            Assert.Contains(transactions, t => t.Id == 1);
            Assert.Contains(transactions, t => t.Id == 2);
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
        /// Tests method <see cref="SplitwiseManager.CompleteTransactionImport"/>.
        /// Verifies that a transaction where the user has no personal amount can be imported as income.
        /// </summary>
        [Fact]
        public void Test_CompleteTransactionImport_Income()
        {
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var user = this.SplitwiseContextMock.GenerateUser(2, "User");

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                date: DateTime.Today.ToLocalDate(),
                paidAmount: 10,
                personalAmount: 5,
                splits: this.context.GenerateSplitDetail(user.Id, 5).Singleton());

            this.context.SaveChanges();

            var transaction = this.SplitwiseManager.CompleteTransactionImport(
                splitwiseTransaction.Id, category.Id, Maybe<int>.None);
            this.RefreshContext();

            Assert.Equal(5, transaction.PersonalAmount);
            Assert.Equal(5, transaction.Amount);
            Assert.True(transaction.Processed);

            // Should have +5
            var splitwiseBalance = this.context.Accounts.GetSplitwiseEntity().CurrentBalance;
            Assert.Equal(5, splitwiseBalance);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.CompleteTransactionImport"/>.
        /// Verifies that a transaction is correctly imported when someone I paid.
        /// </summary>
        [Fact]
        public void Test_CompleteTransactionImport_PaidByMe()
        {
            var (account, _) = this.context.GenerateAccount();
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseUser = this.SplitwiseContextMock.GenerateUser(2, "User2");

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                date: DateTime.Today.ToLocalDate(),
                paidAmount: 10,
                personalAmount: 2.5M,
                splits: this.context.GenerateSplitDetail(splitwiseUser.Id, 7.5M).Singleton());

            this.context.SaveChanges();

            var transaction = this.SplitwiseManager.CompleteTransactionImport(splitwiseTransaction.Id, category.Id, account.Id);

            Assert.Equal(-splitwiseTransaction.PersonalAmount, transaction.PersonalAmount);
            Assert.Equal(-splitwiseTransaction.PaidAmount, transaction.Amount);
            Assert.Equal(category.Id, transaction.CategoryId);
            Assert.Equal(account.Id, transaction.AccountId);
            Assert.Equal(splitwiseTransaction.Description, transaction.Description);
            Assert.Equal(splitwiseTransaction.Date.ToDateString(), transaction.Date);
            Assert.Equal(splitwiseTransaction.Id, transaction.SplitwiseTransactionId);
            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(TransactionType.Expense, transaction.Type);

            this.RefreshContext();

            var accountBalance = this.context.Accounts.GetEntity(account.Id).CurrentBalance;
            var splitwiseBalance = this.context.Accounts.GetEntity(splitwiseAccount.Id).CurrentBalance;

            Assert.Equal(-10, accountBalance);
            Assert.Equal(7.5M, splitwiseBalance);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.CompleteTransactionImport"/>.
        /// Verifies that a transaction is correctly imported when someone paid for me.
        /// </summary>
        [Fact]
        public void Test_CompleteTransactionImport()
        {
            var (account, _) = this.context.GenerateAccount();
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1, date: DateTime.Today.ToLocalDate(), paidAmount: 0, personalAmount: 2.5M);

            this.context.SaveChanges();

            var transaction = this.SplitwiseManager.CompleteTransactionImport(splitwiseTransaction.Id, category.Id, Maybe<int>.None);

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
            this.SplitwiseContextMock.GenerateExpense(
                1, updatedAt: DateTime.MinValue.AddMilliseconds(1), paidAmount: 0);
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
        /// Verifies that transactions for which the user paid an amount are imported if the transaction is not known.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Paid_NotKnown()
        {
            this.GenerateSplitwiseUser();
            this.SplitwiseContextMock.GenerateUser(2, "USER2");
            this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var expense = this.SplitwiseContextMock.GenerateExpense(
                1, paidAmount: 50, personalAmount: 10, splits: new List<Split>
                {
                    this.GenerateSplit(2, 15),
                    this.GenerateSplit(3, 25),
                });

            this.SplitwiseManager.ImportFromSplitwise();

            var splitwiseTransaction = this.context.SplitwiseTransactions.IncludeAll().Single();

            Assert.Equal(expense.Id, splitwiseTransaction.Id);
            Assert.Equal(expense.Description, splitwiseTransaction.Description);
            Assert.Equal(expense.Date, splitwiseTransaction.Date);
            Assert.Equal(expense.PaidAmount, splitwiseTransaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, splitwiseTransaction.PersonalAmount);
            Assert.Equal(expense.UpdatedAt, splitwiseTransaction.UpdatedAt);
            Assert.False(splitwiseTransaction.Imported);

            var splitDetail1 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            var splitDetail2 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(15, splitDetail1.Amount);
            Assert.Equal(25, splitDetail2.Amount);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions for which the user paid an amount are not imported if the transaction is known.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Paid_Known()
        {
            var updatedAt = DateTime.UtcNow;

            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var transaction = this.context.GenerateTransaction(
                account,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                category: category,
                amount: 50,
                splitDetails: splits);
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                updatedAt,
                50,
                10,
                false,
                splits);

            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                updatedAt,
                50,
                10,
                splits.Select(s => s.AsSplit()).ToList());

            this.SplitwiseManager.ImportFromSplitwise();

            this.RefreshContext();

            splitwiseTransaction = this.context.SplitwiseTransactions.IncludeAll().Single();

            Assert.Equal(expense.Id, splitwiseTransaction.Id);
            Assert.Equal(expense.Description, splitwiseTransaction.Description);
            Assert.Equal(expense.Date, splitwiseTransaction.Date);
            Assert.Equal(expense.PaidAmount, splitwiseTransaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, splitwiseTransaction.PersonalAmount);
            Assert.Equal(expense.UpdatedAt, splitwiseTransaction.UpdatedAt);
            Assert.False(splitwiseTransaction.Imported);

            var splitDetail1 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            var splitDetail2 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(15, splitDetail1.Amount);
            Assert.Equal(25, splitDetail2.Amount);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions for which only other people paid are imported.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_NotPaid_NotKnown()
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
        /// Verifies that a transaction for which the user no longer has a share is correctly removed.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_NotPaid_NoShare()
        {
            this.GenerateSplitwiseUser();

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                0,
                10);

            // Add update to expense where user is no longer part of it.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                DateTime.UtcNow,
                0,
                0);

            this.SplitwiseManager.ImportFromSplitwise();

            this.RefreshContext();

            Wv8Assert.IsNone(
                this.context.SplitwiseTransactions.SingleOrNone(st => st.Id == splitwiseTransaction.Id));
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions for which the user did not pay that are already known are properly updated.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_NotPaid()
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
        /// Verifies that transactions for which the user paid that are already known are properly updated.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_Paid()
        {
            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var transaction = this.context.GenerateTransaction(
                account,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                category: category,
                amount: 50,
                splitDetails: splits);
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                50,
                10,
                false,
                splits);

            // Add an update to the transaction, with updated split details.
            var newSplits = new List<Split>
            {
                this.GenerateSplit(user2.Id, 25),
                this.GenerateSplit(user3.Id, 15),
            };
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                "Description123",
                DateTime.Today.ToLocalDate(),
                false,
                DateTime.UtcNow,
                50,
                10,
                newSplits);

            this.SplitwiseManager.ImportFromSplitwise();

            this.RefreshContext();
            splitwiseTransaction = this.context.SplitwiseTransactions.IncludeAll().Single();

            Assert.Equal(expense.Id, splitwiseTransaction.Id);
            Assert.Equal(expense.Description, splitwiseTransaction.Description);
            Assert.Equal(expense.Date, splitwiseTransaction.Date);
            Assert.Equal(expense.PaidAmount, splitwiseTransaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, splitwiseTransaction.PersonalAmount);
            Assert.Equal(expense.UpdatedAt, splitwiseTransaction.UpdatedAt);
            Assert.False(splitwiseTransaction.Imported);

            var splitDetail1 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            var splitDetail2 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(25, splitDetail1.Amount);
            Assert.Equal(15, splitDetail2.Amount);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that updates to a transaction are not processed if the last updated at timestamp is equal to the
        /// one known.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_Paid_EqualLastUpdatedAt()
        {
            var updatedAt = DateTime.UtcNow;

            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var transaction = this.context.GenerateTransaction(
                account,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                category: category,
                amount: 50,
                splitDetails: splits);
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.Today.ToLocalDate(),
                false,
                updatedAt,
                50,
                10,
                false,
                splits);

            // Add an update to the transaction, with updated split details.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                splitwiseTransaction.Id,
                splitwiseTransaction.Description,
                splitwiseTransaction.Date,
                false,
                updatedAt,
                splitwiseTransaction.PaidAmount,
                splitwiseTransaction.PersonalAmount,
                splitwiseTransaction.SplitDetails.Select(sd => sd.AsSplit()).ToList());

            this.SplitwiseManager.ImportFromSplitwise();

            this.RefreshContext();
            splitwiseTransaction = this.context.SplitwiseTransactions.IncludeAll().Single();

            Assert.Equal(expense.Id, splitwiseTransaction.Id);
            Assert.Equal(expense.Description, splitwiseTransaction.Description);
            Assert.Equal(expense.Date, splitwiseTransaction.Date);
            Assert.Equal(expense.PaidAmount, splitwiseTransaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, splitwiseTransaction.PersonalAmount);
            Assert.Equal(expense.UpdatedAt, splitwiseTransaction.UpdatedAt);
            Assert.False(splitwiseTransaction.Imported);

            var splitDetail1 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            var splitDetail2 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(15, splitDetail1.Amount);
            Assert.Equal(25, splitDetail2.Amount);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions that are already known are properly updated, even if they are already completely
        /// imported and processed.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_Paid_AlreadyProcessed()
        {
            // Add processed Splitwise transaction to database.
            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.UtcNow.AddDays(-7),
                paidAmount: 50,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                imported: true,
                splits: splits);
            this.context.GenerateTransaction(
                account,
                TransactionType.Expense,
                splitwiseTransaction.Description,
                splitwiseTransaction.Date,
                splitwiseTransaction.PaidAmount,
                category,
                splitwiseTransaction: splitwiseTransaction,
                splitDetails: splits);
            this.SaveAndProcess();

            // Add new version to mock. In future so should not be processed.
            var newSplits = new List<Split>
            {
                this.GenerateSplit(user2.Id, 10),
                this.GenerateSplit(user3.Id, 25),
            };
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.UtcNow,
                paidAmount: 50,
                personalAmount: 15,
                date: DateTime.Today.AddDays(1).ToLocalDate(),
                description: "Description2",
                splits: newSplits);
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(0, accountBalance);

            splitwiseTransaction = this.context.SplitwiseTransactions.GetEntity(splitwiseTransaction.Id);

            Assert.Equal(expense.Id, splitwiseTransaction.Id);
            Assert.Equal(expense.Description, splitwiseTransaction.Description);
            Assert.Equal(expense.Date, splitwiseTransaction.Date);
            Assert.Equal(expense.PaidAmount, splitwiseTransaction.PaidAmount);
            Assert.Equal(expense.PersonalAmount, splitwiseTransaction.PersonalAmount);
            Assert.Equal(expense.UpdatedAt, splitwiseTransaction.UpdatedAt);
            Assert.True(splitwiseTransaction.Imported);

            var splitDetail1 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            var splitDetail2 = splitwiseTransaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(10, splitDetail1.Amount);
            Assert.Equal(25, splitDetail2.Amount);

            var transaction = this.context.Transactions.IncludeAll().Single();

            Assert.Equal(transaction.Description, splitwiseTransaction.Description);
            Assert.Equal(transaction.Date, splitwiseTransaction.Date);
            Assert.Equal(transaction.Amount, -splitwiseTransaction.PaidAmount);
            Assert.Equal(transaction.PersonalAmount, -splitwiseTransaction.PersonalAmount);
            Assert.False(transaction.Processed);

            splitDetail1 = transaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 2);
            splitDetail2 = transaction.SplitDetails.Single(sd => sd.SplitwiseUserId == 3);

            Assert.Equal(10, splitDetail1.Amount);
            Assert.Equal(25, splitDetail2.Amount);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that transactions that are already known are properly updated, even if they are already completely
        /// imported and processed.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Update_NotPaid_AlreadyProcessed()
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
        public void Test_ImportFromSplitwise_Removed_Paid()
        {
            // Add processed Splitwise transaction to database.
            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: DateTime.Now.AddDays(-7),
                paidAmount: 50,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                imported: true,
                splits: splits);
            this.context.GenerateTransaction(
                splitwiseAccount,
                TransactionType.Expense,
                splitwiseTransaction.Description,
                splitwiseTransaction.Date,
                splitwiseTransaction.PaidAmount,
                category,
                splitwiseTransaction: splitwiseTransaction,
                splitDetails: splits);
            this.SaveAndProcess();

            // Add new version to mock. Deleted.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: DateTime.UtcNow,
                paidAmount: splitwiseTransaction.PaidAmount,
                personalAmount: splitwiseTransaction.PersonalAmount,
                date: splitwiseTransaction.Date,
                description: splitwiseTransaction.Description,
                splits: splitwiseTransaction.SplitDetails.Select(sd => sd.AsSplit()).ToList(),
                isDeleted: true);
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(0, accountBalance);

            splitwiseTransaction = this.context.SplitwiseTransactions.Single(st => st.Id == splitwiseTransaction.Id);
            Assert.True(splitwiseTransaction.IsDeleted);
            Assert.False(splitwiseTransaction.Imported);

            Wv8Assert.IsNone(this.context.Transactions.SingleOrNone(t => t.SplitwiseTransactionId == splitwiseTransaction.Id));
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that an import of a removed transaction is not processed when the last updated at is equal to the
        /// one known.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Removed_Paid_EqualLastUpdatedAt()
        {
            var updatedAt = DateTime.UtcNow;

            // Add processed Splitwise transaction to database.
            this.GenerateSplitwiseUser();
            var user2 = this.SplitwiseContextMock.GenerateUser(2, "USER2");
            var user3 = this.SplitwiseContextMock.GenerateUser(3, "USER3");

            var (account, _) = this.context.GenerateAccount();
            var split1 = this.context.GenerateSplitDetail(user2.Id, 15);
            var split2 = this.context.GenerateSplitDetail(user3.Id, 25);
            var splits = new List<SplitDetailEntity> { split1, split2 };
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                updatedAt: updatedAt,
                paidAmount: 50,
                personalAmount: 10,
                date: DateTime.Today.ToLocalDate(),
                description: "Description",
                isDeleted: true,
                splits: splits);
            this.context.SaveChanges();

            // Add expense which is deleted. Equal updated at.
            var expense = this.SplitwiseContextMock.GenerateExpense(
                1,
                updatedAt: updatedAt,
                paidAmount: splitwiseTransaction.PaidAmount,
                personalAmount: splitwiseTransaction.PersonalAmount,
                date: splitwiseTransaction.Date,
                description: splitwiseTransaction.Description,
                splits: splitwiseTransaction.SplitDetails.Select(sd => sd.AsSplit()).ToList(),
                isDeleted: true);
            this.SplitwiseManager.ImportFromSplitwise();

            // Verify revert
            this.RefreshContext();

            splitwiseTransaction = this.context.SplitwiseTransactions.GetEntity(splitwiseTransaction.Id);

            Assert.True(splitwiseTransaction.IsDeleted);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that a known transaction is removed properly.
        /// </summary>
        [Fact]
        public void Test_ImportFromSplitwise_Removed_NotPaid()
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
        public void Test_ImportFromSplitwise_Removed_NotPaid_AlreadyProcessed()
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
        public void Test_ImportFromSplitwise_Removed_NotKnown()
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

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.ImportFromSplitwise"/>.
        /// Verifies that the importer can only run once.
        /// </summary>
        [Fact]
        public async void Test_ImportFromSplitwise_RunOnce()
        {
            this.SplitwiseContextMock.ExtraTimeWhenImporting = true;

            var task = Task.Factory.StartNew(() =>
            {
                var result1 = this.SplitwiseManager.ImportFromSplitwise();
                Assert.Equal(ImportResult.Completed, result1);
            });

            // Sleep so that the first import task is started before this one.
            Thread.Sleep(250);

            var result2 = this.SplitwiseManager.ImportFromSplitwise();
            Assert.Equal(ImportResult.AlreadyRunning, result2);

            await task;
        }

        #endregion ImportFromSplitwise

        #region GetSplitwiseUsers

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.GetSplitwiseUsers"/>.
        /// Verifies that the name is correctly set.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseUsers_ConcatName()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", "van Acht");

            var users = this.SplitwiseManager.GetSplitwiseUsers();

            var user = users.Single();
            Assert.Equal(1, user.Id);
            Assert.Equal("Wouter van Acht", user.Name);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.GetSplitwiseUsers"/>.
        /// Verifies that the name is correctly set.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseUsers_NoLastName()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", null);

            var users = this.SplitwiseManager.GetSplitwiseUsers();

            var user = users.Single();
            Assert.Equal(1, user.Id);
            Assert.Equal("Wouter", user.Name);
        }

        #endregion GetSplitwiseUsers

        #region GetImporterInformation

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.GetImporterInformation"/>.
        /// Verifies that The last run value is correctly set.
        /// </summary>
        [Fact]
        public void Test_GetImporterInformation_LastRun()
        {
            var timestamp = new DateTime(2021, 03, 07, 15, 40, 10);
            this.SetSplitwiseLastRunTime(timestamp);
            this.context.SaveChanges();

            var information = this.SplitwiseManager.GetImporterInformation();

            Assert.Equal(timestamp.ToDateTimeString(), information.LastRunTimestamp);
            Assert.Equal(ImportState.NotRunning, information.CurrentState);
        }

        /// <summary>
        /// Tests method <see cref="SplitwiseManager.GetImporterInformation"/>.
        /// Verifies that the state is correctly returned when the importer is running.
        /// </summary>
        [Fact]
        public async void Test_GetImporterInformation_CurrentlyRunning()
        {
            this.SplitwiseContextMock.ExtraTimeWhenImporting = true;

            var task = Task.Factory.StartNew(() => this.SplitwiseManager.ImportFromSplitwise());

            // Sleep so that the import task is actually started.
            Thread.Sleep(250);

            var information = this.SplitwiseManager.GetImporterInformation();
            Assert.Equal(ImportState.Running, information.CurrentState);

            await task;
        }

        #endregion GetImporterInformation

        private void SetSplitwiseLastRunTime(DateTime timestamp)
        {
            this.context.SynchronizationTimes.Single().SplitwiseLastRun = timestamp;
        }

        private User GenerateSplitwiseUser()
        {
            return this.SplitwiseContextMock.GenerateUser(this.splitwiseUserId, "User");
        }

        private Split GenerateSplit(int userId = 0, decimal amount = 10, string userName = "User")
        {
            return new Split
            {
                UserId = userId,
                Amount = amount,
                UserName = userName,
            };
        }
    }
}
