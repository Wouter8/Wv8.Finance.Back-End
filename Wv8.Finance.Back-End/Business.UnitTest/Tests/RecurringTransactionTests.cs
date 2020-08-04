namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using NodaTime;
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
                categoryId: category.Id);
            var rTransaction2 = this.GenerateRecurringTransaction(
                accountId: account1.Id,
                type: TransactionType.Internal,
                receivingAccountId: account2.Id);
            var finishedRecurringTransactions = this.GenerateRecurringTransaction(
                accountId: account1.Id,
                startDate: LocalDate.FromDateTime(DateTime.Today).PlusDays(-7),
                endDate: LocalDate.FromDateTime(DateTime.Today),
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
                    TransactionType.Expense, Maybe<int>.None, Maybe<int>.None, false);
            Assert.Single(retrieved);
        }

        #endregion GetRecurringTransactionsByFilter

        #region UpdateRecurringTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.UpdateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateRecurringTransaction()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(7); // 3 instances should be created, finished
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

            Assert.True(rTransaction.Finished);
            Assert.Equal(3, instances.Count);

            var newAccount = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1).ToDateString();
            var newEndDate = LocalDate.FromDateTime(DateTime.Today).ToDateString();
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
        /// Tests the good flow of the <see cref="RecurringTransactionManager.UpdateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateRecurringTransaction_NoEndDate()
        {
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(7); // 3 instances should be created, finished
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

            Assert.True(rTransaction.Finished);
            Assert.Equal(3, instances.Count);

            var newAccount = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1).ToDateString();
            var newEndDate = Maybe<string>.None;
            var newAmount = -30;
            var newCategory = this.GenerateCategory().Id;
            var newInterval = 1;
            var newIntervalUnit = IntervalUnit.Days; // Should be 9 instances created

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

            Assert.False(updated.Finished);
            Assert.True(updated.NextOccurence.IsSome);
            Assert.Equal(9, instances.Count);

            instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            t.NeedsConfirmation) // Verify new instances are created
                .ToList();
            Assert.Equal(9, instances.Count);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="RecurringTransactionManager.UpdateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateRecurringTransaction_Exceptions()
        {
            var account = this.GenerateAccount().Id;
            var account2 = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(7); // 2 instances should be created, not finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account,
                description,
                startDate.ToDateString(),
                endDate.ToDateString(),
                amount,
                category,
                Maybe<int>.None,
                interval,
                intervalUnit,
                false);

            // Try to update start date without updating instances.
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);

            Assert.Throws<ValidationException>(() =>
                this.RecurringTransactionManager.UpdateRecurringTransaction(
                    rTransaction.Id,
                    account,
                    description,
                    newStartDate.ToString(),
                    endDate.ToString(),
                    amount,
                    category,
                    Maybe<int>.None,
                    interval,
                    intervalUnit,
                    false,
                    false));

            // Try to update type of transaction
            Assert.Throws<ValidationException>(() =>
                this.RecurringTransactionManager.UpdateRecurringTransaction(
                    rTransaction.Id,
                    account,
                    description,
                    newStartDate.ToString(),
                    endDate.ToString(),
                    amount,
                    Maybe<int>.None,
                    account2,
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
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(7); // 3 instances should be created, finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account,
                description,
                startDate.ToDateString(),
                endDate.ToDateString(),
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

            Assert.True(rTransaction.Finished);
            Assert.Equal(3, instances.Count);
        }

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.CreateRecurringTransaction"/> method.
        /// </summary>
        [Fact]
        public void CreateRecurringTransaction_NoEndDate()
        {
            var account = this.GenerateAccount().Id;
            var description = "Description";
            var amount = -30;
            var category = this.GenerateCategory().Id;
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = Maybe<string>.None; // 3 instances should be created, finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account,
                description,
                startDate.ToDateString(),
                endDate,
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
            Assert.Equal(endDate, rTransaction.EndDate);
            Assert.Equal(3, instances.Count);
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
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-7);
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(7); // 3 instances should be created, finished
            var interval = 1;
            var intervalUnit = IntervalUnit.Weeks;

            var rTransaction = this.RecurringTransactionManager.CreateRecurringTransaction(
                account.Id,
                description,
                startDate.ToDateString(),
                endDate.ToDateString(),
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
                description,
                startDate.ToDateString(),
                endDate.ToDateString(),
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
            Assert.Equal(3, instances.Count);
        }

        #endregion DeleteRecurringTransaction
    }
}