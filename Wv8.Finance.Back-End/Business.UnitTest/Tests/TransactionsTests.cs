namespace Business.UnitTest.Tests
{
    using System;
    using System.Linq;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common.Enums;
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
            var account1 = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var categoryIncome = this.GenerateCategory(CategoryType.Income);
            var categoryExpense = this.GenerateCategory();
            var categoryChild = this.GenerateCategory(parentCategoryId: categoryExpense.Id);

            // Create income transactions.
            var transaction1 = this.GenerateTransaction(
                account1.Id,
                TransactionType.Income,
                "Income",
                DateTime.Today.AddDays(1),
                100,
                categoryIncome.Id);
            var transaction2 = this.GenerateTransaction(
                account2.Id,
                TransactionType.Income,
                "Expense",
                DateTime.Today,
                200,
                categoryIncome.Id);

            // Retrieve.
            var result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);

            // Assert.
            Assert.Equal(300, result.TotalSum);
            var correspondingCategory = result.SumPerIncomeCategory.Keys.Single(c => c.Id == categoryIncome.Id);
            Assert.Equal(300, result.SumPerIncomeCategory[correspondingCategory]);
            Assert.Equal(2, result.TransactionsPerType[TransactionType.Income].Count);

            // Retrieve by date.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                DateTime.Today.AddDays(1).ToString("O"),
                DateTime.Today.AddDays(1).ToString("O"),
                0,
                100);

            // Assert.
            Assert.Equal(100, result.TotalSum);
            correspondingCategory = result.SumPerIncomeCategory.Keys.Single(c => c.Id == categoryIncome.Id);
            Assert.Equal(100, result.SumPerIncomeCategory[correspondingCategory]);
            Assert.Single(result.TransactionsPerType[TransactionType.Income]);

            // Create expense transactions.
            var transaction3 = this.GenerateTransaction(
                accountId: account1.Id,
                type: TransactionType.Expense,
                date: DateTime.Today.AddDays(2),
                amount: -200,
                categoryId: categoryExpense.Id);
            var transaction4 = this.GenerateTransaction(
                accountId: account1.Id,
                type: TransactionType.Expense,
                date: DateTime.Today.AddDays(3),
                amount: -20,
                categoryId: categoryChild.Id);

            // Retrieve by date.
            result = this.TransactionManager.GetTransactionsByFilter(
                Maybe<TransactionType>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                DateTime.Today.AddDays(2).ToString("O"),
                DateTime.Today.AddDays(5).ToString("O"),
                0,
                100);

            // Assert.
            Assert.Equal(-220, result.TotalSum);
            correspondingCategory = result.SumPerExpenseCategory.Keys.Single(c => c.Id == categoryExpense.Id);
            Assert.Equal(-200, result.SumPerExpenseCategory[correspondingCategory]);
            Assert.Single(result.TransactionsPerCategory[correspondingCategory]);
            correspondingCategory = result.SumPerExpenseCategory.Keys.Single(c => c.Id == categoryChild.Id);
            Assert.Equal(-20, result.SumPerExpenseCategory[correspondingCategory]);
            Assert.Single(result.TransactionsPerCategory[correspondingCategory]);
            Assert.Equal(2, result.TransactionsPerType[TransactionType.Expense].Count);
            Assert.Equal(2, result.Transactions.Count);

            // Retrieve by type.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.Expense,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Equal(2, result.Transactions.Count);

            // Retrieve with pagination.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.Expense,
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
                "   ex   ",
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Single(result.Transactions);
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
            var newDate = DateTime.Today.AddDays(-1);
            var newAmount = -10;
            var newCategoryId = this.GenerateCategory().Id;
            var newBudgetId = this.GenerateBudget(
                categoryId: newCategoryId,
                startDate: newDate.AddDays(-1)).Id;

            // Update.
            var updated = this.TransactionManager.UpdateTransaction(
                transaction.Id,
                newAccountId,
                newDescription,
                newDate.ToString("O"),
                newAmount,
                newCategoryId,
                Maybe<int>.None);

            // Assert.
            Assert.Equal(transaction.Id, updated.Id);
            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newDate, DateTime.Parse(updated.Date));
            Assert.Equal(newAmount, updated.Amount);
            Assert.Equal(newCategoryId, updated.CategoryId.Value);
            Assert.True(updated.Settled);

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
                type: TransactionType.Transfer,
                amount: 50,
                receivingAccountId: receiver.Id);

            var newReceiver = this.GenerateAccount();

            // Update.
            updated = this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                sender.Id,
                transferTransaction.Description,
                DateTime.Today.AddDays(1).ToString("O"), // Future
                transferTransaction.Amount,
                Maybe<int>.None,
                newReceiver.Id);

            // Assert
            Assert.Equal(newReceiver.Id, updated.ReceivingAccountId.Value);
            Assert.False(updated.Settled);

            sender = this.AccountManager.GetAccount(sender.Id);
            receiver = this.AccountManager.GetAccount(receiver.Id);
            newReceiver = this.AccountManager.GetAccount(newReceiver.Id);

            // Shouldn't be settled because update is in future.
            Assert.Equal(0, sender.CurrentBalance);
            Assert.Equal(0, receiver.CurrentBalance);
            Assert.Equal(0, newReceiver.CurrentBalance);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="ITransactionManager.UpdateTransaction"/> method.
        /// </summary>
        [Fact]
        public void UpdateTransaction_Exceptions()
        {
            var expenseCategory = this.GenerateCategory();
            var incomeCategory = this.GenerateCategory(CategoryType.Income);
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var expenseTransaction = this.GenerateTransaction(account.Id);
            var incomeTransaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Income,
                categoryId: incomeCategory.Id);
            var transferTransaction = this.GenerateTransaction(account.Id, TransactionType.Transfer);

            var description = "Description";
            var date = DateTime.Today.ToString("O");
            var amount = 20;

            /* Type errors */
            // Amount positive on expense transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                expenseCategory.Id,
                Maybe<int>.None));
            // No category specified on expense.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // Amount negative on income transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                incomeTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                incomeCategory.Id,
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
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
            this.AccountManager.SetAccountObsolete(account.Id, false);

            /* Category mismatch */
            // Income category on expense transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                incomeCategory.Id,
                Maybe<int>.None));
            // Expense category on income transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                incomeTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                expenseCategory.Id,
                Maybe<int>.None));

            /* Category obsolete */
            // Income category on expense transaction.
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, true);
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
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
            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
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
            var type = TransactionType.Expense;
            var description = "Description";
            var date = DateTime.Today.AddDays(-1);
            var amount = -10;
            var categoryId = this.GenerateCategory().Id;
            var budgetId = this.GenerateBudget(
                categoryId: categoryId,
                startDate: date.AddDays(-1)).Id;

            // Create.
            var created = this.TransactionManager.CreateTransaction(
                accountId,
                type,
                description,
                date.ToString("O"),
                amount,
                categoryId,
                Maybe<int>.None);

            // Assert.
            Assert.Equal(type, created.Type);
            Assert.Equal(description, created.Description);
            Assert.Equal(date, DateTime.Parse(created.Date));
            Assert.Equal(amount, created.Amount);
            Assert.Equal(categoryId, created.CategoryId.Value);
            Assert.True(created.Settled);

            budget = this.BudgetManager.GetBudget(budgetId);
            account = this.AccountManager.GetAccount(accountId);
            Assert.Equal(Math.Abs(amount), budget.Spent);
            Assert.Equal(amount, account.CurrentBalance);

            // Test updating transfer transaction.
            var sender = this.GenerateAccount();
            var receiver = this.GenerateAccount();

            // Create.
            created = this.TransactionManager.CreateTransaction(
                sender.Id,
                TransactionType.Transfer,
                description,
                DateTime.Today.AddDays(1).ToString("O"), // Future
                50,
                Maybe<int>.None,
                receiver.Id);

            // Assert
            Assert.Equal(receiver.Id, created.ReceivingAccountId.Value);
            Assert.False(created.Settled);

            sender = this.AccountManager.GetAccount(sender.Id);
            receiver = this.AccountManager.GetAccount(receiver.Id);

            // Shouldn't be settled because update is in future.
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
            var incomeCategory = this.GenerateCategory(CategoryType.Income);
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            var description = "Description";
            var date = DateTime.Today.ToString("O");
            var amount = 20;

            /* Type errors */
            // Amount positive on expense transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                date,
                amount,
                expenseCategory.Id,
                Maybe<int>.None));
            // No category specified on expense.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // Amount negative on income transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Income,
                description,
                date,
                -amount,
                incomeCategory.Id,
                Maybe<int>.None));
            // No category specified on income.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Income,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // Amount negative on transfer transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Transfer,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id));
            // No receiver specified on transfer.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Transfer,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None));

            /* Account obsolete */
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
            this.AccountManager.SetAccountObsolete(account.Id, false);

            /* Category mismatch */
            // Income category on expense transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                date,
                -amount,
                incomeCategory.Id,
                Maybe<int>.None));
            // Expense category on income transaction.
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Income,
                description,
                date,
                amount,
                expenseCategory.Id,
                Maybe<int>.None));

            /* Category obsolete */
            // Income category on expense transaction.
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, true);
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Expense,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
            this.CategoryManager.SetCategoryObsolete(expenseCategory.Id, false);

            /* Receiving account obsolete. */
            this.AccountManager.SetAccountObsolete(account2.Id, true);
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Transfer,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id));
            this.AccountManager.SetAccountObsolete(account2.Id, false);

            /* Sender same as receiver */
            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(
                account.Id,
                TransactionType.Transfer,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account.Id));
        }

        #endregion CreateTransaction

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
                date: DateTime.Today);

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