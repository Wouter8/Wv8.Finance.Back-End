﻿namespace Business.UnitTest.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Business.UnitTest.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Collections;
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
            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var rTransaction = this.context.GenerateRecurringTransaction(account, category: category);
            this.SaveAndProcess();

            var retrieved = this.RecurringTransactionManager.GetRecurringTransaction(rTransaction.Id);

            this.AssertEqual(rTransaction.AsRecurringTransaction(), retrieved);
        }

        #endregion GetRecurringTransaction

        #region GetRecurringTransactionsByFilter

        /// <summary>
        /// Tests the good flow of the <see cref="RecurringTransactionManager.GetRecurringTransactionsByFilter"/> method.
        /// </summary>
        [Fact]
        public void GetRecurringTransactionsByFilter()
        {
            var (account1, _) = this.context.GenerateAccount();
            var (account2, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var rTransaction1 = this.context.GenerateRecurringTransaction(
                account1, category: category);
            var rTransaction2 = this.context.GenerateRecurringTransaction(
                account1,
                TransactionType.Transfer,
                receivingAccount: account2);
            var finishedRecurringTransactions = this.context.GenerateRecurringTransaction(
                account1,
                startDate: LocalDate.FromDateTime(DateTime.Today).PlusDays(-7),
                endDate: LocalDate.FromDateTime(DateTime.Today),
                category: category);

            this.SaveAndProcess();

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

            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();

            var rTransaction = this.context.GenerateRecurringTransaction(
                account,
                category: category,
                startDate: startDate,
                endDate: endDate,
                interval: interval,
                intervalUnit: intervalUnit);
            this.SaveAndProcess();
            this.RefreshContext();

            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.True(rTransaction.Finished);
            Assert.Equal(3, instances.Count);

            var newAccount = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var newEndDate = LocalDate.FromDateTime(DateTime.Today);
            var newAmount = -30;
            var newCategory = this.GenerateCategory().Id;
            var newInterval = 1;
            var newIntervalUnit = IntervalUnit.Days; // Should be 2 instances created

            var updated = this.RecurringTransactionManager.UpdateRecurringTransaction(
                rTransaction.Id,
                this.GetInputRecurringTransaction(
                    newAccount,
                    TransactionType.Expense,
                    newDescription,
                    newStartDate,
                    newEndDate,
                    newAmount,
                    newCategory,
                    null,
                    true,
                    newInterval,
                    newIntervalUnit),
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

            var (account, _) = this.context.GenerateAccount();
            var category = this.context.GenerateCategory();
            var rTransaction = this.context.GenerateRecurringTransaction(
                account,
                category: category,
                startDate: startDate,
                endDate: endDate,
                interval: interval,
                intervalUnit: intervalUnit);

            this.SaveAndProcess();
            this.RefreshContext();

            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.True(rTransaction.Finished);
            Assert.Equal(3, instances.Count);

            var newAccount = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var newEndDate = (LocalDate?)null;
            var newAmount = -30;
            var newCategory = this.GenerateCategory().Id;
            var newInterval = 1;
            var newIntervalUnit = IntervalUnit.Days; // Should be 9 instances created

            var updated = this.RecurringTransactionManager.UpdateRecurringTransaction(
                rTransaction.Id,
                this.GetInputRecurringTransaction(
                newAccount,
                TransactionType.Expense,
                newDescription,
                newStartDate,
                newEndDate,
                newAmount,
                newCategory,
                null,
                true,
                newInterval,
                newIntervalUnit),
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
                this.GetInputRecurringTransaction(
                    account,
                    TransactionType.Expense,
                    description,
                    startDate,
                    endDate,
                    amount,
                    category,
                    null,
                    false,
                    interval,
                    intervalUnit));

            // Try to update start date without updating instances.
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);

            Assert.Throws<ValidationException>(() =>
                this.RecurringTransactionManager.UpdateRecurringTransaction(
                    rTransaction.Id,
                    this.GetInputRecurringTransaction(
                        account,
                        TransactionType.Expense,
                        description,
                        newStartDate,
                        endDate,
                        amount,
                        category,
                        null,
                        false,
                        interval,
                        intervalUnit),
                    false));
        }

        /// <summary>
        /// Tests method <see cref="IRecurringTransactionManager.UpdateRecurringTransaction"/>.
        /// Verifies that an exception is thrown in all cases where the input is incorrect.
        /// </summary>
        [Fact]
        public void Test_UpdateRecurringTransaction_SplitwiseValidation()
        {
            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var transaction = this.context.GenerateRecurringTransaction(account, category: category);

            var splitwiseUser = this.SplitwiseContextMock.GenerateUser(1, "User1");

            this.context.SaveChanges();

            var input = transaction.ToInput();
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 25, UserId = 1 }.Singleton();

            // Wrong transaction type
            input.Amount = 100;
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.UpdateRecurringTransaction(transaction.Id, input, true),
                "Payment requests and Splitwise splits can only be specified on expenses.");

            // Splits greater than amount
            input.Amount = -10;
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.UpdateRecurringTransaction(transaction.Id, input, true),
                "The amount split can not exceed the total amount of the transaction.");

            input.Amount = -100;

            // Split without amount
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 0, UserId = 1 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.UpdateRecurringTransaction(transaction.Id, input, true),
                "Splits must have an amount greater than 0.");

            // 2 splits for same user
            input.SplitwiseSplits = new List<InputSplitwiseSplit>
            {
                new InputSplitwiseSplit { Amount = 10, UserId = 1 },
                new InputSplitwiseSplit { Amount = 20, UserId = 1 },
            };
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.UpdateRecurringTransaction(transaction.Id, input, true),
                "A user can only be linked to a single split.");

            // Unknown Splitwise user
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 10, UserId = 2 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.UpdateRecurringTransaction(transaction.Id, input, true),
                "Unknown Splitwise user(s) specified.");
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
                this.GetInputRecurringTransaction(
                    account,
                    TransactionType.Expense,
                    description,
                    startDate,
                    endDate,
                    amount,
                    category,
                    null,
                    false,
                    interval,
                    intervalUnit));

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
                this.GetInputRecurringTransaction(
                    account,
                    TransactionType.Expense,
                    description,
                    startDate,
                    null,
                    amount,
                    category,
                    null,
                    false,
                    interval,
                    intervalUnit));

            var instances = this.context.Transactions
                .Where(t => t.RecurringTransactionId == rTransaction.Id &&
                            !t.NeedsConfirmation) // Verify needs confirmation property
                .ToList();

            Assert.False(rTransaction.Finished);
            Assert.Equal(endDate, rTransaction.EndDate);
            Assert.Equal(3, instances.Count);
        }

        /// <summary>
        /// Tests method <see cref="IRecurringTransactionManager.CreateRecurringTransaction"/>.
        /// Verifies that an exception is thrown in all cases where the input is incorrect.
        /// </summary>
        [Fact]
        public void Test_CreateRecurringTransaction_SplitwiseValidation()
        {
            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseUser = this.SplitwiseContextMock.GenerateUser(1, "User1");

            this.context.SaveChanges();

            // Wrong transaction type
            var input = this.GetInputRecurringTransaction(
                account.Id,
                TransactionType.Income,
                categoryId: category.Id,
                amount: 50,
                splitwiseSplits: new InputSplitwiseSplit { Amount = 25, UserId = 1 }.Singleton());
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.CreateRecurringTransaction(input),
                "Payment requests and Splitwise splits can only be specified on expenses.");

            // Splits greater than amount
            input.Amount = -10;
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.CreateRecurringTransaction(input),
                "The amount split can not exceed the total amount of the transaction.");

            input.Amount = -100;

            // Split without amount
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 0, UserId = 1 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.CreateRecurringTransaction(input),
                "Splits must have an amount greater than 0.");

            // 2 splits for same user
            input.SplitwiseSplits = new List<InputSplitwiseSplit>
            {
                new InputSplitwiseSplit { Amount = 10, UserId = 1 },
                new InputSplitwiseSplit { Amount = 20, UserId = 1 },
            };
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.CreateRecurringTransaction(input),
                "A user can only be linked to a single split.");

            // Unknown Splitwise user
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 10, UserId = 2 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.RecurringTransactionManager.CreateRecurringTransaction(input),
                "Unknown Splitwise user(s) specified.");
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
                this.GetInputRecurringTransaction(
                    account.Id,
                    TransactionType.Expense,
                    description,
                    startDate,
                    endDate,
                    amount,
                    category,
                    null,
                    false,
                    interval,
                    intervalUnit));

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
                this.GetInputRecurringTransaction(
                    account.Id,
                    TransactionType.Expense,
                    description,
                    startDate,
                    endDate,
                    amount,
                    category,
                    null,
                    false,
                    interval,
                    intervalUnit));

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