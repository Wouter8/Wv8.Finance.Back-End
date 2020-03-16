namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A test class testing the <see cref="RecurringTransactionManager"/>.
    /// </summary>
    public class RecurringTransactionTests : BaseTest
    {
        #region GetRecurringTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.GetRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void GetRecurringTransaction()
        {
            var rTransaction = this.GenerateRecurringTransaction();
            var retrieved = this.RecurringTransactionManager.GetRecurringTransaction(rTransaction.Id);

            this.AssertEqual(rTransaction, retrieved);
        }

        #endregion GetRecurringTransaction

        #region GetRecurringTransactionsByFilter

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.GetRecurringTransactionsByFilter"/> method.
        /// </summary>
        [Fact]
        public void GetRecurringTransactionsByFilter()
        {
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();
            var category = this.GenerateCategory();
            var rTransaction1 = this.GenerateRecurringTransaction(
                accountId: account1.Id,
                type: TransactionType.Expense,
                categoryId: category.Id);
            var rTransaction2 = this.GenerateRecurringTransaction(
                accountId: account1.Id,
                type: TransactionType.Transfer,
                receivingAccountId: account2.Id);
            var finishedRecurringTransactions = this.GenerateRecurringTransaction(
                accountId: account1.Id,
                type: TransactionType.Expense,
                startDate: DateTime.Today.AddDays(-7),
                endDate: DateTime.Today,
                categoryId: category.Id);

            // No filters
            var retrieved =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    Maybe<TransactionType>.None, Maybe<int>.None, Maybe<int>.None, true);
            Assert.Equal(3, retrieved.Count);

            // Only unfinished
            retrieved =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    Maybe<TransactionType>.None, Maybe<int>.None, Maybe<int>.None, false);
            Assert.Equal(2, retrieved.Count);

            // Category filter
            retrieved =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    Maybe<TransactionType>.None, Maybe<int>.None, category.Id, false);
            Assert.Single(retrieved);

            // Account filter
            retrieved =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    Maybe<TransactionType>.None, account2.Id, Maybe<int>.None, false);
            Assert.Single(retrieved);

            // Type filter
            retrieved =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    TransactionType.Income, Maybe<int>.None, Maybe<int>.None, false);
            Assert.Empty(retrieved);
        }

        #endregion GetRecurringTransactionsByFilter

        #region UpdateRecurringTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.UpdateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateRecurringTransaction()
        {
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today.AddDays(7); // 2 instances should be created, not finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.GenerateRecurringTransaction(
                startDate: startDate,
                endDate: endDate,
                interval: interval,
                intervalUnit: intervalUnit);
            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.False(rTransaction.Finished);
            Assert.Equal(endDate.ToIsoString(), rTransaction.NextOccurence.Value);
            Assert.Equal(2, instances.Count);

            var newAccount = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newStartDate = DateTime.Today.AddDays(-1).ToIsoString();
            var newEndDate = DateTime.Today.ToIsoString();
            var newAmount = -30;
            var newCategory = this.GenerateCategory().Id;
            var newInterval = 1;
            var newIntervalUnit = IntervalUnit.Days; // Should be 2 instances created

            var updated = this.RecurringTransactionManager.UpdateRecurringTransaction(
                rTransaction.Id,
                newAccount,
                newDescription,
                newStartDate,
                newEndDate,
                newAmount,
                newCategory,
                Maybe<int>.None,
                newInterval,
                newIntervalUnit,
                true,
                true);

            instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id) // No check on need confirmation to see if old instances ar deleted.
                .ToList();

            Assert.True(updated.Finished);
            Assert.False(updated.NextOccurence.IsSome);
            Assert.Equal(2, instances.Count);

            instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            t.NeedsConfirmation) // Verify new instances are created
                .ToList();
            Assert.Equal(2, instances.Count);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="RecurringTransactionManager.UpdateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateRecurringTransaction_Exceptions()
        {
            var account = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today.AddDays(7); // 2 instances should be created, not finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account,
                TransactionType.Expense,
                description,
                startDate.ToIsoString(),
                endDate.ToIsoString(),
                amount,
                category,
                Maybe<int>.None,
                interval,
                intervalUnit,
                false);

            // Try to update start date without updating instances.
            var newStartDate = DateTime.Today.AddDays(-1);

            Assert.Throws<ValidationException>(() =>
                this.RecurringTransactionManager.UpdateRecurringTransaction(
                    rTransaction.Id,
                    account,
                    description,
                    newStartDate.ToIsoString(),
                    endDate.ToIsoString(),
                    amount,
                    category,
                    Maybe<int>.None,
                    interval,
                    intervalUnit,
                    false,
                    false));
        }

        #endregion UpdateRecurringTransaction

        #region CreateRecurringTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.CreateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void CreateRecurringTransaction()
        {
            var account = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today.AddDays(7); // 2 instances should be created, not finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account,
                TransactionType.Expense,
                description,
                startDate.ToIsoString(),
                endDate.ToIsoString(),
                amount,
                category,
                Maybe<int>.None,
                interval,
                intervalUnit,
                false);

            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.False(rTransaction.Finished);
            Assert.Equal(endDate.ToIsoString(), rTransaction.NextOccurence.Value);
            Assert.Equal(2, instances.Count);
        }

        #endregion CreateRecurringTransaction

        #region DeleteRecurringTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.DeleteRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void DeleteRecurringTransaction()
        {
            var account = this.GenerateAccount();
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = DateTime.Today.AddDays(-7);
            var endDate = DateTime.Today.AddDays(7); // 2 instances should be created, not finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                startDate.ToIsoString(),
                endDate.ToIsoString(),
                amount,
                category,
                Maybe<int>.None,
                interval,
                intervalUnit,
                false);

            // Delete recurring transaction and delete 2 instances.
            this.RecurringTransactionManager.DeleteRecurringTransaction(rTransaction.Id, true);
            Assert.Throws<DoesNotExistException>(() =>
                this.RecurringTransactionManager.GetRecurringTransaction(rTransaction.Id));

            // Verify instances are deleted and account balance is restored.
            var instances = this.context.Transactions.ToList();
            account = this.AccountManager.GetAccount(account.Id);
            Assert.Equal(0, account.CurrentBalance);
            Assert.Empty(instances);

            rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                startDate.ToIsoString(),
                endDate.ToIsoString(),
                amount,
                category,
                Maybe<int>.None,
                interval,
                intervalUnit,
                false);

            // Delete recurring transaction but leave 2 instances.
            this.RecurringTransactionManager.DeleteRecurringTransaction(rTransaction.Id, false);
            Assert.Throws<DoesNotExistException>(() =>
                this.RecurringTransactionManager.GetRecurringTransaction(rTransaction.Id));

            // Verify instances are not deleted (although link to recurring transaction is removed).
            instances = this.context.Transactions.ToList();
            Assert.Equal(2, instances.Count);
        }

        #endregion DeleteRecurringTransaction
    }
}