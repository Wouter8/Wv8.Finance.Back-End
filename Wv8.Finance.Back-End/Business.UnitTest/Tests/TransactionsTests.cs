namespace Business.UnitTest.Tests
{
    using System;
    using NodaTime;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="TransactionManager"/>.
    /// </summary>
    public class TransactionsTests : BaseTest
    {
        #region GetTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.GetTransaction"/> method.
        /// </summary>
        [Fact]
        public void GetTransaction()
        {
            var transaction = this.GenerateTransaction();

            var retrievedTransaction = this.TransactionManager.GetTransaction(transaction.Id);

            this.AssertEqual(transaction, retrievedTransaction);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.GetTransaction"/> method.
        /// </summary>
        [Fact]
        public void GetTransaction_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.TransactionManager.GetTransaction(100));
        }

        #endregion GetTransaction

        #region GetTransactionsByFilter

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.GetTransactionsByFilter"/> method.
        /// </summary>
        [Fact]
        public void GetTransactionsByFilter()
        {
            // Generate objects.
            var account1 = this.GenerateAccount("AAA");
            var account2 = this.GenerateAccount("BBB");

            var categoryIncome = this.GenerateCategory(description: "CCC");
            var categoryExpense = this.GenerateCategory(description: "DDD");
            var categoryChild = this.GenerateCategory(
                description: "FFF", parentCategoryId: categoryExpense.Id);

            // Create income transactions.
            var transaction1 = this.GenerateTransaction(
                account1.Id,
                TransactionType.External,
                "Income",
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1),
                100,
                categoryIncome.Id);
            var transaction2 = this.GenerateTransaction(
                account2.Id,
                TransactionType.External,
                "Expense",
                LocalDate.FromDateTime(DateTime.Today),
                200,
                categoryIncome.Id);

            // Retrieve.
            var result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);

            // Assert.
            Assert.Equal(300, result.TotalSum);
            Assert.Equal(300, result.SumPerCategory[categoryIncome.Id]);
            Assert.Equal(2, result.TransactionsPerType[TransactionType.External].Count);

            // Retrieve by date.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(),
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(),
                0,
                100);

            // Assert.
            Assert.Equal(100, result.TotalSum);
            Assert.Equal(100, result.SumPerCategory[categoryIncome.Id]);
            Assert.Single(result.TransactionsPerType[TransactionType.External]);

            // Create expense transactions.
            var transaction3 = this.GenerateTransaction(
                account1.Id,
                TransactionType.External,
                "DDD",
                LocalDate.FromDateTime(DateTime.Today).PlusDays(2),
                -200,
                categoryExpense.Id);
            var transaction4 = this.GenerateTransaction(
                account1.Id,
                TransactionType.External,
                "FFF",
                LocalDate.FromDateTime(DateTime.Today).PlusDays(3),
                -20,
                categoryChild.Id);

            // Retrieve by date.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                LocalDate.FromDateTime(DateTime.Today).PlusDays(2).ToDateString(),
                LocalDate.FromDateTime(DateTime.Today).PlusDays(5).ToDateString(),
                0,
                100);

            // Assert.
            Assert.Equal(-220, result.TotalSum);
            Assert.Equal(-200, result.SumPerCategory[categoryExpense.Id]);
            Assert.Single(result.TransactionsPerCategory[categoryExpense.Id]);
            Assert.Equal(-20, result.SumPerCategory[categoryChild.Id]);
            Assert.Single(result.TransactionsPerCategory[categoryChild.Id]);
            Assert.Equal(2, result.TransactionsPerType[TransactionType.External].Count);
            Assert.Equal(2, result.Transactions.Count);

            // Retrieve by type.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.External,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Equal(4, result.Transactions.Count);

            // Retrieve with pagination.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.External,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                1);
            Assert.Single(result.Transactions);

            // Retrieve by description.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<int>.None,
                "   e   ",
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Equal(2, result.Transactions.Count);

            // Retrieve by description.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<int>.None,
                "   ex   ",
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Single(result.Transactions);

            // Create transfer transaction.
            var transaction5 = this.GenerateTransaction(
                accountId: account1.Id,
                type: TransactionType.Internal,
                date: LocalDate.FromDateTime(DateTime.Today),
                amount: 200,
                receivingAccountId: account2.Id);

            // Retrieve by account.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                account2.Id,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Equal(2, result.Transactions.Count);
        }

        #endregion GetTransaction

        #region UpdateTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.UpdateTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateTransaction()
        {
            // Generate objects.
            var category = this.GenerateCategory();
            var budget = this.GenerateBudget(category.Id);
            var account = this.GenerateAccount();

            var transaction = this.GenerateTransaction(accountId: account.Id, categoryId: category.Id);

            // New values.
            var newAccountId = this.GenerateAccount().Id;
            var newDescription = "Description";
            var newDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var newAmount = -10;
            var newCategoryId = this.GenerateCategory().Id;
            var newBudgetId = this.GenerateBudget(
                categoryId: newCategoryId,
                startDate: newDate.PlusDays(-1)).Id;

            // Update.
            var updated = this.TransactionManager.UpdateTransaction(
                transaction.Id,
                newAccountId,
                newDescription,
                newDate.ToDateString(),
                newAmount,
                newCategoryId,
                Maybe<int>.None);

            // Assert.
            Assert.Equal(transaction.Id, updated.Id);
            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newDate.ToDateString(), updated.Date);
            Assert.Equal(newAmount, updated.Amount);
            Assert.Equal(newCategoryId, updated.CategoryId.Value);
            Assert.True(updated.Processed);

            var oldBudget = this.BudgetManager.GetBudget(budget.Id);
            var oldAccount = this.AccountManager.GetAccount(account.Id);
            Assert.Equal(0, oldBudget.Spent);
            Assert.Equal(0, oldAccount.CurrentBalance);

            var newBudget = this.BudgetManager.GetBudget(newBudgetId);
            var newAccount = this.AccountManager.GetAccount(newAccountId);
            Assert.Equal(Math.Abs(newAmount), newBudget.Spent);
            Assert.Equal(newAmount, newAccount.CurrentBalance);

            // Test updating transfer transaction.
            var sender = this.GenerateAccount();
            var receiver = this.GenerateAccount();
            var transferTransaction = this.GenerateTransaction(
                accountId: sender.Id,
                type: TransactionType.Internal,
                amount: 50,
                receivingAccountId: receiver.Id);

            var newReceiver = this.GenerateAccount();

            // Update.
            updated = this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                sender.Id,
                transferTransaction.Description,
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(), // Future
                transferTransaction.Amount,
                Maybe<int>.None,
                newReceiver.Id);

            // Assert
            Assert.Equal(newReceiver.Id, updated.ReceivingAccountId.Value);
            Assert.False(updated.Processed);

            sender = this.AccountManager.GetAccount(sender.Id);
            receiver = this.AccountManager.GetAccount(receiver.Id);
            newReceiver = this.AccountManager.GetAccount(newReceiver.Id);

            // Shouldn't be processed because update is in future.
            Assert.Equal(0, sender.CurrentBalance);
            Assert.Equal(0, receiver.CurrentBalance);
            Assert.Equal(0, newReceiver.CurrentBalance);

            // Transaction that needs confirmation.
            var toBeConfirmedTransaction = this.GenerateTransaction(needsConfirmation: true);
            updated = this.TransactionManager.UpdateTransaction(
                toBeConfirmedTransaction.Id,
                newAccountId,
                newDescription,
                newDate.ToDateString(),
                newAmount,
                newCategoryId,
                Maybe<int>.None);

            // Shouldn't be processed since transaction is not confirmed yet.
            Assert.False(updated.Processed);

            // Change date to future and confirm
            updated = this.TransactionManager.UpdateTransaction(
                toBeConfirmedTransaction.Id,
                newAccountId,
                newDescription,
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(),
                newAmount,
                newCategoryId,
                Maybe<int>.None);
            updated = this.TransactionManager.ConfirmTransaction(
                toBeConfirmedTransaction.Id, LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(), newAmount);
            // Now change date to past
            updated = this.TransactionManager.UpdateTransaction(
                toBeConfirmedTransaction.Id,
                newAccountId,
                newDescription,
                LocalDate.FromDateTime(DateTime.Today).ToDateString(),
                newAmount,
                newCategoryId,
                Maybe<int>.None);
            Assert.True(updated.Processed);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.UpdateTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateTransaction_Exceptions()
        {
            var expenseCategory = this.GenerateCategory();
            var incomeCategory = this.GenerateCategory();
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var expenseTransaction = this.GenerateTransaction(account.Id);
            var incomeTransaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.External,
                amount: 50,
                categoryId: incomeCategory.Id);
            var transferTransaction = this.GenerateTransaction(account.Id, TransactionType.Internal);

            var description = "Description";
            var date = LocalDate.FromDateTime(DateTime.Today).ToDateString();
            var amount = 20;

            /* Type errors */
            // No category specified on expense.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // No category specified on income.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                incomeTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // Amount negative on transfer transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id));
            // No receiver specified on transfer.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None));

            /* Account obsolete */
            // Fix current balance to 0 with new transaction
            var fixTransaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.External,
                amount: 50,
                categoryId: incomeCategory.Id);
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
            this.AccountManager.SetAccountObsolete(account.Id, false);
            this.TransactionManager.DeleteTransaction(fixTransaction.Id);

            /* Category obsolete */
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, false);

            /* Receiving account obsolete. */
            this.AccountManager.SetAccountObsolete(account2.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                account2.Id));
            this.AccountManager.SetAccountObsolete(account2.Id, false);

            /* Sender same as receiver */
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account.Id));

            /* Try to update type of transaction */

            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
        }

        #endregion UpdateTransaction

        #region UpdateTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.CreateTransaction"/> method.
        /// </summary>
        [Fact]
        public void CreateTransaction()
        {
            // Generate objects.
            var category = this.GenerateCategory();
            var budget = this.GenerateBudget(category.Id);
            var account = this.GenerateAccount();

            // Values.
            var accountId = this.GenerateAccount().Id;
            var type = TransactionType.External;
            var description = "Description";
            var date = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var amount = -10;
            var categoryId = this.GenerateCategory().Id;
            var budgetId = this.GenerateBudget(
                categoryId: categoryId,
                startDate: date.PlusDays(-1)).Id;

            // Create.
            var created = this.TransactionManager.CreateTransaction(
                accountId,
                description,
                date.ToDateString(),
                amount,
                categoryId,
                Maybe<int>.None,
                false);

            // Assert.
            Assert.Equal(type, created.Type);
            Assert.Equal(description, created.Description);
            Assert.Equal(date.ToDateString(), created.Date);
            Assert.Equal(amount, created.Amount);
            Assert.Equal(categoryId, created.CategoryId.Value);
            Assert.True(created.Processed);

            budget = this.BudgetManager.GetBudget(budgetId);
            account = this.AccountManager.GetAccount(accountId);
            Assert.Equal(Math.Abs(amount), budget.Spent);
            Assert.Equal(amount, account.CurrentBalance);

            // Test transfer transaction.
            var sender = this.GenerateAccount();
            var receiver = this.GenerateAccount();

            // Create.
            created = this.TransactionManager.CreateTransaction(
                sender.Id,
                description,
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToDateString(), // Future
                50,
                Maybe<int>.None,
                receiver.Id,
                false);

            // Assert
            Assert.Equal(receiver.Id, created.ReceivingAccountId.Value);
            Assert.False(created.Processed);

            // Shouldn't be processed because transaction is in future.
            sender = this.AccountManager.GetAccount(sender.Id);
            receiver = this.AccountManager.GetAccount(receiver.Id);
            Assert.Equal(0, sender.CurrentBalance);
            Assert.Equal(0, receiver.CurrentBalance);

            // Test transaction that needs to be confirmed.
            // Create.
            created = this.TransactionManager.CreateTransaction(
                sender.Id,
                description,
                LocalDate.FromDateTime(DateTime.Today).ToDateString(),
                50,
                Maybe<int>.None,
                receiver.Id,
                true);

            // Assert
            Assert.True(created.NeedsConfirmation);
            Assert.False(created.IsConfirmed.Value);
            Assert.False(created.Processed);

            // Shouldn't be processed because transaction is unconfirmed.
            sender = this.AccountManager.GetAccount(sender.Id);
            receiver = this.AccountManager.GetAccount(receiver.Id);
            Assert.Equal(0, sender.CurrentBalance);
            Assert.Equal(0, receiver.CurrentBalance);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.CreateTransaction"/> method.
        /// </summary>
        [Fact]
        public void CreateTransaction_Exceptions()
        {
            var expenseCategory = this.GenerateCategory();
            var incomeCategory = this.GenerateCategory();
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var description = "Description";
            var date = LocalDate.FromDateTime(DateTime.Today).ToDateString();
            var amount = 20;

            /* Type errors */
            // No category specified on expense.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));
            // No category specified on income.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));
            // Amount negative on transfer transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id,
                false));
            // No receiver specified on transfer.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));

            /* Account obsolete */
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None,
                false));
            this.AccountManager.SetAccountObsolete(account.Id, false);

            /* Category obsolete */
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None,
                false));
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, false);

            /* Receiving account obsolete. */
            this.AccountManager.SetAccountObsolete(account2.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                account2.Id,
                false));
            this.AccountManager.SetAccountObsolete(account2.Id, false);

            /* Sender same as receiver */
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account.Id,
                false));
        }

        #endregion CreateTransaction

        #region ConfirmTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.ConfirmTransaction"/> method.
        /// </summary>
        [Fact]
        public void ConfirmTransaction()
        {
            // Add to be confirmed transaction
            var transaction = this.GenerateTransaction(needsConfirmation: true);

            var confirmedAmount = -50;
            var confirmedDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);

            var updated = this.TransactionManager.ConfirmTransaction(
                transaction.Id, confirmedDate.ToDateString(), confirmedAmount);

            // Assert.
            Assert.Equal(confirmedDate.ToDateString(), updated.Date);
            Assert.Equal(confirmedAmount, updated.Amount);
            Assert.True(updated.Processed);

            // Test confirmation in the future.
            confirmedDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(3);
            var transaction2 = this.GenerateTransaction(needsConfirmation: true);
            updated = this.TransactionManager.ConfirmTransaction(
                transaction2.Id, confirmedDate.ToDateString(), confirmedAmount);

            // Assert.
            Assert.Equal(confirmedDate.ToDateString(), updated.Date);
            Assert.Equal(confirmedAmount, updated.Amount);
            Assert.False(updated.Processed);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.CreateTransaction"/> method.
        /// </summary>
        [Fact]
        public void ConfirmTransaction_Exceptions()
        {
            var account = this.GenerateAccount();
            var category = this.GenerateCategory();
            var receivingAccount = this.GenerateAccount();

            // Add to be confirmed transaction
            var transaction = new TransactionEntity
            {
                AccountId = account.Id,
                Amount = -45,
                CategoryId = category.Id,
                Date = LocalDate.FromDateTime(DateTime.Today),
                Description = "Description",
                IsConfirmed = false,
                NeedsConfirmation = true,
                Processed = false,
                Type = TransactionType.External,
            };
            // Transaction that doesn't need confirmation
            var transactionNoConfirmation = new TransactionEntity
            {
                AccountId = account.Id,
                Amount = -45,
                CategoryId = category.Id,
                Date = LocalDate.FromDateTime(DateTime.Today),
                Description = "Description",
                NeedsConfirmation = false,
                Processed = false,
                Type = TransactionType.External,
            };
            // Transfer transaction
            var transferTransaction = new TransactionEntity
            {
                AccountId = account.Id,
                Amount = -45,
                ReceivingAccountId = receivingAccount.Id,
                Date = LocalDate.FromDateTime(DateTime.Today),
                Description = "Description",
                IsConfirmed = false,
                NeedsConfirmation = true,
                Processed = false,
                Type = TransactionType.Internal,
            };

            this.context.Transactions.Add(transaction);
            this.context.Transactions.Add(transactionNoConfirmation);
            this.context.Transactions.Add(transferTransaction);
            this.context.SaveChanges();

            var confirmedAmount = -50;
            var confirmedDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);

            Assert.Throws<InvalidOperationException>(
                () => this.TransactionManager.ConfirmTransaction(
                    transactionNoConfirmation.Id, confirmedDate.ToDateString(), confirmedAmount));

            // Account obsolete
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<IsObsoleteException>(
                () => this.TransactionManager.ConfirmTransaction(
                    transaction.Id, confirmedDate.ToDateString(), confirmedAmount));
            this.AccountManager.SetAccountObsolete(account.Id, false);

            // Receiving account obsolete
            this.AccountManager.SetAccountObsolete(receivingAccount.Id, true);
            Assert.Throws<IsObsoleteException>(
                () => this.TransactionManager.ConfirmTransaction(
                    transferTransaction.Id, confirmedDate.ToDateString(), -confirmedAmount));
            this.AccountManager.SetAccountObsolete(receivingAccount.Id, false);

            // Category obsolete
            this.CategoryManager.SetCategoryObsolete(category.Id, true);
            Assert.Throws<IsObsoleteException>(
                () => this.TransactionManager.ConfirmTransaction(
                    transaction.Id, confirmedDate.ToDateString(), confirmedAmount));
            this.CategoryManager.SetCategoryObsolete(category.Id, false);
        }

        #endregion ConfirmTransaction

        #region DeleteTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.DeleteTransaction"/> method.
        /// </summary>
        [Fact]
        public void DeleteTransaction()
        {
            var account = this.GenerateAccount();

            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                amount: -20,
                date: LocalDate.FromDateTime(DateTime.Today));

            this.TransactionManager.DeleteTransaction(transaction.Id);

            account = this.AccountManager.GetAccount(account.Id);
            Assert.Throws<DoesNotExistException>(() => this.TransactionManager.GetTransaction(transaction.Id));
            Assert.Equal(0, account.CurrentBalance);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.DeleteTransaction"/> method.
        /// </summary>
        [Fact]
        public void DeleteTransaction_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.TransactionManager.DeleteTransaction(100));
        }

        #endregion DeleteTransaction
    }
}