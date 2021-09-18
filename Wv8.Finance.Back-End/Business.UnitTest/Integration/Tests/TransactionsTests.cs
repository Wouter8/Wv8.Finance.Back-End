namespace Business.UnitTest.Integration.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Business.UnitTest.Integration.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="TransactionManager"/>.
    /// </summary>
    public class TransactionsTests : BaseIntegrationTest
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
            var account1 = this.GenerateAccount(description: "AAA");
            var account2 = this.GenerateAccount(description: "BBB");

            var category = this.GenerateCategory(description: "CCC");
            var categoryChild = this.GenerateCategory(
                description: "FFF", parentCategoryId: category.Id);

            // Create income transactions.
            var transaction1 = this.GenerateTransaction(
                account1.Id,
                TransactionType.Income,
                "Income",
                LocalDate.FromDateTime(DateTime.Today).PlusDays(1),
                100,
                category.Id);
            var transaction2 = this.GenerateTransaction(
                account2.Id,
                TransactionType.Expense,
                "Expense",
                LocalDate.FromDateTime(DateTime.Today),
                -200,
                category.Id);

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
            Assert.Equal(-100, result.TotalSum);
            Assert.Equal(-100, result.SumPerCategory[category.Id]);
            Assert.Single(result.TransactionsPerType[TransactionType.Expense]);
            Assert.Single(result.TransactionsPerType[TransactionType.Income]);

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
            Assert.Equal(100, result.SumPerCategory[category.Id]);
            Assert.Single(result.TransactionsPerType[TransactionType.Income]);

            // Create expense transactions.
            var transaction3 = this.GenerateTransaction(
                account1.Id,
                TransactionType.Expense,
                "DDD",
                LocalDate.FromDateTime(DateTime.Today).PlusDays(2),
                -200,
                category.Id);
            var transaction4 = this.GenerateTransaction(
                account1.Id,
                TransactionType.Expense,
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
            // TODO: This category should have a sum of 220 because the transactions of the child category should be
            // taken into account as well.
            Assert.Equal(-200, result.SumPerCategory[category.Id]);
            Assert.Single(result.TransactionsPerCategory[category.Id]);
            Assert.Equal(-20, result.SumPerCategory[categoryChild.Id]);
            Assert.Single(result.TransactionsPerCategory[categoryChild.Id]);
            Assert.Equal(2, result.TransactionsPerType[TransactionType.Expense].Count);
            Assert.Equal(2, result.Transactions.Count);

            // Retrieve by type.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.Expense,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                0,
                100);
            Assert.Equal(3, result.Transactions.Count);

            // Retrieve with pagination.
            result = this.TransactionManager.GetTransactionsByFilter(
                TransactionType.Expense,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<int>.None,
                Maybe<string>.None,
                Maybe<string>.None,
                2,
                3);
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
                type: TransactionType.Transfer,
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
        public void Test_UpdateTransaction()
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
            var newAmount = -10m;
            var newCategoryId = this.GenerateCategory().Id;
            var newBudgetId = this.GenerateBudget(
                categoryId: newCategoryId,
                startDate: newDate.PlusDays(-1)).Id;

            // Update.
            var updated = this.UpdateTransaction(
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
                type: TransactionType.Transfer,
                amount: 50,
                receivingAccountId: receiver.Id);

            var newReceiver = this.GenerateAccount();

            // Update.
            updated = this.UpdateTransaction(
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
            updated = this.UpdateTransaction(
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
            updated = this.UpdateTransaction(
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
            updated = this.UpdateTransaction(
                toBeConfirmedTransaction.Id,
                newAccountId,
                newDescription,
                LocalDate.FromDateTime(DateTime.Today).ToDateString(),
                newAmount,
                newCategoryId,
                Maybe<int>.None);
            Assert.True(updated.Processed);
        }

        // TODO: Exception tests should validate that the correct exception is being tested. Either by message or derived exception types.

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
                type: TransactionType.Income,
                amount: 50,
                categoryId: incomeCategory.Id);
            var transferTransaction = this.GenerateTransaction(account.Id, TransactionType.Transfer);

            var description = "Description";
            var date = LocalDate.FromDateTime(DateTime.Today).ToDateString();
            var amount = 20;

            /* Type errors */
            // No category specified on expense.
            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
                expenseTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // No category specified on income.
            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
                incomeTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None));
            // Amount negative on transfer transaction.
            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id));
            // No receiver specified on transfer.
            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
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
                type: TransactionType.Income,
                amount: 50,
                categoryId: incomeCategory.Id);
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.UpdateTransaction(
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
            Assert.Throws<IsObsoleteException>(() => this.UpdateTransaction(
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
            Assert.Throws<IsObsoleteException>(() => this.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                account2.Id));
            this.AccountManager.SetAccountObsolete(account2.Id, false);

            /* Sender same as receiver */
            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account.Id));

            /* Try to update type of transaction */

            Assert.Throws<ValidationException>(() => this.UpdateTransaction(
                transferTransaction.Id,
                account.Id,
                description,
                date,
                -amount,
                expenseCategory.Id,
                Maybe<int>.None));
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.UpdateTransaction"/> method. Verifies that an exception is thrown
        /// when the account of the transaction that is updated is a Splitwise account.
        /// </summary>
        [Fact]
        public void UpdateTransaction_SplitwiseAccount()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var transaction = this.context.GenerateTransaction(account, category: category, amount: -50);
            var (normalAccount, _) = this.context.GenerateAccount();
            this.context.SaveChanges();

            var edit = transaction.ToInput();
            edit.AccountId = normalAccount.Id;

            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(transaction.Id, edit));
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.UpdateTransaction"/> method. Verifies that an exception is thrown
        /// when the new account of the transaction that is updated is a Splitwise account.
        /// </summary>
        [Fact]
        public void UpdateTransaction_SplitwiseAccount2()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var (normalAccount, _) = this.context.GenerateAccount();
            var transaction = this.context.GenerateTransaction(normalAccount, category: category, amount: -50);
            this.context.SaveChanges();

            var edit = transaction.ToInput();
            edit.AccountId = account.Id;

            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(transaction.Id, edit));
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.UpdateTransaction"/> method. Verifies that an exception is thrown
        /// when the account of the transaction that is updated is a Splitwise account.
        /// </summary>
        [Fact]
        public void UpdateTransaction_Transfer_SplitwiseAccount()
        {
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var (normalAccount, _) = this.context.GenerateAccount();
            var (normalAccount2, _) = this.context.GenerateAccount();
            var transaction = this.context.GenerateTransaction(
                normalAccount, TransactionType.Transfer, receivingAccount: account, amount: 50);
            this.context.SaveChanges();

            var edit = transaction.ToInput();
            edit.ReceivingAccountId = normalAccount2.Id;

            Assert.Throws<ValidationException>(() => this.TransactionManager.UpdateTransaction(transaction.Id, edit));
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.UpdateTransaction"/>.
        /// Verifies that payment requests are correctly updated.
        /// </summary>
        [Fact]
        public void Test_UpdateTransaction_PaymentRequests()
        {
            var account = this.GenerateAccount();
            var category = this.GenerateCategory();

            var input = new InputTransaction
            {
                AccountId = account.Id,
                Description = "Transaction",
                DateString = DateTime.Today.ToDateString(),
                Amount = -500,
                CategoryId = category.Id,
                ReceivingAccountId = Maybe<int>.None,
                NeedsConfirmation = false,
                PaymentRequests = new List<InputPaymentRequest>
                {
                    new InputPaymentRequest
                    {
                        Amount = 50,
                        Count = 4,
                        Name = "Group",
                    },
                    new InputPaymentRequest
                    {
                        Amount = 200,
                        Count = 1,
                        Name = "Person",
                    },
                },
                SplitwiseSplits = new List<InputSplitwiseSplit>(),
            };

            var transaction = this.TransactionManager.CreateTransaction(input);
            var prGroup = transaction.PaymentRequests.Single(pr => pr.Count == 4);
            var prPerson = transaction.PaymentRequests.Single(pr => pr.Count == 1);

            var edit = new InputTransaction
            {
                Amount = transaction.Amount,
                Description = transaction.Description,
                AccountId = transaction.AccountId,
                CategoryId = transaction.CategoryId,
                DateString = transaction.Date,
                ReceivingAccountId = Maybe<int>.None,
                PaymentRequests = new List<InputPaymentRequest>
                {
                    new InputPaymentRequest
                    {
                        Id = prGroup.Id,
                        Amount = 50,
                        Count = 6,
                        Name = "Group",
                    },
                    new InputPaymentRequest
                    {
                        Id = Maybe<int>.None,
                        Amount = 100,
                        Count = 1,
                        Name = "Person",
                    },
                },
                SplitwiseSplits = new List<InputSplitwiseSplit>(),
            };
            transaction = this.TransactionManager.UpdateTransaction(transaction.Id, edit);
            var prIds = transaction.PaymentRequests.Select(pr => pr.Id).ToList();

            Assert.DoesNotContain(prPerson.Id, prIds);
            Assert.Contains(prGroup.Id, prIds);

            account = this.AccountManager.GetAccount(account.Id);

            Assert.Equal(2, transaction.PaymentRequests.Count);
            Assert.Equal(400, transaction.PaymentRequests.Sum(pr => pr.AmountDue));
            Assert.Equal(-100, transaction.PersonalAmount);

            Assert.Equal(-500, account.CurrentBalance);

            // TODO: Add exception tests for payment requests
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.UpdateTransaction"/>.
        /// Verifies that an exception is thrown in all cases where the input is incorrect.
        /// </summary>
        [Fact]
        public void Test_UpdateTransaction_SplitwiseValidation()
        {
            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var transaction = this.context.GenerateTransaction(account, category: category);

            var splitwiseUser = this.SplitwiseContextMock.GenerateUser(1, "User1");

            this.context.SaveChanges();

            var input = transaction.ToInput();
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 25, UserId = 1 }.Singleton();

            // Wrong transaction type
            input.Amount = 100;
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "Payment requests and Splitwise splits can only be specified on expenses.");

            // Splits greater than amount
            input.Amount = -10;
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "The amount split can not exceed the total amount of the transaction.");

            input.Amount = -100;

            // Split without amount
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 0, UserId = 1 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "Splits must have an amount greater than 0.");

            // 2 splits for same user
            input.SplitwiseSplits = new List<InputSplitwiseSplit>
            {
                new InputSplitwiseSplit { Amount = 10, UserId = 1 },
                new InputSplitwiseSplit { Amount = 20, UserId = 1 },
            };
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "A user can only be linked to a single split.");

            // Unknown Splitwise user
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 10, UserId = 2 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "Unknown Splitwise user(s) specified.");
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.UpdateTransaction"/>.
        /// Verifies that payment requests are correctly updated.
        /// </summary>
        [Fact]
        public void Test_UpdateTransaction_SplitwiseSplits()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", "van Acht");
            this.SplitwiseContextMock.GenerateUser(2, "Jeroen");

            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var transactionEntity = this.context.GenerateTransaction(account, amount: -300, category: category);

            this.context.SaveChanges();

            var input = transactionEntity.ToInput();
            input.SplitwiseSplits = new List<InputSplitwiseSplit>
            {
                new InputSplitwiseSplit
                {
                    Amount = 100,
                    UserId = 1,
                },
                new InputSplitwiseSplit
                {
                    Amount = 150,
                    UserId = 2,
                },
            };

            var transaction = this.TransactionManager.UpdateTransaction(transactionEntity.Id, input);

            this.RefreshContext();

            var expense =
                this.SplitwiseContextMock.Expenses.Single(e => e.Id == transaction.SplitwiseTransaction.Value.Id);
            account = this.context.Accounts.GetEntity(account.Id);

            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(250, transaction.SplitwiseTransaction.Value.OwedByOthers);
            Assert.Equal(50, transaction.SplitwiseTransaction.Value.PersonalAmount);
            Assert.Equal(300, transaction.SplitwiseTransaction.Value.PaidAmount);
            Assert.Equal(-50, transaction.PersonalAmount);

            Assert.Equal(transaction.SplitwiseTransactionId.Value, expense.Id);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PaidAmount, expense.PaidAmount);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PersonalAmount, expense.PersonalAmount);
            Assert.Equal(transaction.Date, expense.Date.ToDateString());
            Assert.False(expense.IsDeleted);

            Assert.Equal(-300, account.CurrentBalance);
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.UpdateTransaction"/>.
        /// Verifies that an exception is thrown if the transaction is not editable.
        /// </summary>
        [Fact]
        public void Test_UpdateTransaction_NotEditable()
        {
            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(paidAmount: 0, personalAmount: 10);
            var transaction = this.context.GenerateTransaction(
                account, category: category, amount: -10, splitwiseTransaction: splitwiseTransaction);

            this.SplitwiseContextMock.GenerateUser(1, "User1");

            this.context.SaveChanges();

            var input = transaction.ToInput();

            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.UpdateTransaction(transaction.Id, input),
                "This transaction should be updated in Splitwise.");
        }

        #endregion UpdateTransaction

        #region CreateTransaction

        /// <summary>
        /// Tests the good flow of the <see cref="ITransactionManager.CreateTransaction"/> method.
        /// </summary>
        [Fact]
        public void Test_CreateTransaction()
        {
            // Generate objects.
            var category = this.GenerateCategory();
            var budget = this.GenerateBudget(category.Id);
            var account = this.GenerateAccount();

            // Values.
            var accountId = this.GenerateAccount().Id;
            var type = TransactionType.Expense;
            var description = "Description";
            var date = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1);
            var amount = -10;
            var categoryId = this.GenerateCategory().Id;
            var budgetId = this.GenerateBudget(
                categoryId: categoryId,
                startDate: date.PlusDays(-1)).Id;

            // Create.
            var created = this.CreateTransaction(
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
            created = this.CreateTransaction(
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
            created = this.CreateTransaction(
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
            Assert.Throws<ValidationException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));
            // No category specified on income.
            Assert.Throws<ValidationException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));
            // Amount negative on transfer transaction.
            Assert.Throws<ValidationException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account2.Id,
                false));
            // No receiver specified on transfer.
            Assert.Throws<ValidationException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                Maybe<int>.None,
                false));

            /* Account obsolete */
            this.AccountManager.SetAccountObsolete(account.Id, true);
            Assert.Throws<IsObsoleteException>(() => this.CreateTransaction(
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
            Assert.Throws<IsObsoleteException>(() => this.CreateTransaction(
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
            Assert.Throws<IsObsoleteException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                amount,
                Maybe<int>.None,
                account2.Id,
                false));
            this.AccountManager.SetAccountObsolete(account2.Id, false);

            /* Sender same as receiver */
            Assert.Throws<ValidationException>(() => this.CreateTransaction(
                account.Id,
                description,
                date,
                -amount,
                Maybe<int>.None,
                account.Id,
                false));
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.CreateTransaction"/> method. Verifies that an exception is thrown
        /// when the account of the transaction is a Splitwise account.
        /// </summary>
        [Fact]
        public void CreateTransaction_SplitwiseAccount()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            this.context.SaveChanges();

            var input = this.GetInputTransaction(account.Id, categoryId: category.Id);

            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(input));
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.CreateTransaction"/> method. Verifies that an exception is thrown
        /// when the receiving account of the transaction is a Splitwise account.
        /// </summary>
        [Fact]
        public void CreateTransaction_Transfer_SplitwiseAccount()
        {
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var (normalAccount, _) = this.context.GenerateAccount();
            this.context.SaveChanges();

            var input = this.GetInputTransaction(normalAccount.Id, TransactionType.Transfer, receivingAccountId: account.Id);

            Assert.Throws<ValidationException>(() => this.TransactionManager.CreateTransaction(input));
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.CreateTransaction"/>.
        /// Verifies that payment requests are correctly created.
        /// </summary>
        [Fact]
        public void Test_CreateTransaction_PaymentRequests()
        {
            var account = this.GenerateAccount();
            var category = this.GenerateCategory();

            var input = new InputTransaction
            {
                AccountId = account.Id,
                Description = "Transaction",
                DateString = DateTime.Today.ToDateString(),
                Amount = -500,
                CategoryId = category.Id,
                ReceivingAccountId = Maybe<int>.None,
                NeedsConfirmation = false,
                PaymentRequests = new List<InputPaymentRequest>
                {
                    new InputPaymentRequest
                    {
                        Amount = 50,
                        Count = 4,
                        Name = "Group",
                    },
                    new InputPaymentRequest
                    {
                        Amount = 200,
                        Count = 1,
                        Name = "Person",
                    },
                },
                SplitwiseSplits = new List<InputSplitwiseSplit>(),
            };

            var transaction = this.TransactionManager.CreateTransaction(input);
            account = this.AccountManager.GetAccount(account.Id);

            Assert.Equal(2, transaction.PaymentRequests.Count);
            Assert.Equal(400, transaction.PaymentRequests.Sum(pr => pr.AmountDue));
            Assert.Equal(-100, transaction.PersonalAmount);

            Assert.Equal(-500, account.CurrentBalance);
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.CreateTransaction"/>.
        /// Verifies that an exception is thrown in all cases where the input is incorrect.
        /// </summary>
        [Fact]
        public void Test_CreateTransaction_SplitwiseValidation()
        {
            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            var splitwiseUser = this.SplitwiseContextMock.GenerateUser(1, "User1");

            this.context.SaveChanges();

            // Wrong transaction type
            var input = this.GetInputTransaction(
                account.Id,
                TransactionType.Income,
                categoryId: category.Id,
                amount: 50,
                splitwiseSplits: new InputSplitwiseSplit { Amount = 25, UserId = 1 }.Singleton());
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.CreateTransaction(input),
                "Payment requests and Splitwise splits can only be specified on expenses.");

            // Splits greater than amount
            input.Amount = -10;
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.CreateTransaction(input),
                "The amount split can not exceed the total amount of the transaction.");

            input.Amount = -100;

            // Split without amount
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 0, UserId = 1 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.CreateTransaction(input),
                "Splits must have an amount greater than 0.");

            // 2 splits for same user
            input.SplitwiseSplits = new List<InputSplitwiseSplit>
            {
                new InputSplitwiseSplit { Amount = 10, UserId = 1 },
                new InputSplitwiseSplit { Amount = 20, UserId = 1 },
            };
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.CreateTransaction(input),
                "A user can only be linked to a single split.");

            // Unknown Splitwise user
            input.SplitwiseSplits = new InputSplitwiseSplit { Amount = 10, UserId = 2 }.Singleton();
            Wv8Assert.Throws<ValidationException>(
                () => this.TransactionManager.CreateTransaction(input),
                "Unknown Splitwise user(s) specified.");
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.CreateTransaction"/>.
        /// Verifies that a transaction with specified Splitwise splits is correctly created.
        /// </summary>
        [Fact]
        public void Test_CreateTransaction_SplitwiseSplits()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", "van Acht");
            this.SplitwiseContextMock.GenerateUser(2, "Jeroen");

            var (account, _) = this.context.GenerateAccount();
            var splitwiseAccount = this.context.GenerateAccount(AccountType.Splitwise);
            var category = this.context.GenerateCategory();

            this.context.SaveChanges();

            var input = new InputTransaction
            {
                AccountId = account.Id,
                Description = "Transaction",
                DateString = DateTime.Today.ToDateString(),
                Amount = -300,
                CategoryId = category.Id,
                ReceivingAccountId = Maybe<int>.None,
                NeedsConfirmation = false,
                PaymentRequests = new List<InputPaymentRequest>(),
                SplitwiseSplits = new List<InputSplitwiseSplit>
                {
                    new InputSplitwiseSplit
                    {
                        Amount = 100,
                        UserId = 1,
                    },
                    new InputSplitwiseSplit
                    {
                        Amount = 150,
                        UserId = 2,
                    },
                },
            };

            var transaction = this.TransactionManager.CreateTransaction(input);

            this.RefreshContext();

            var expense =
                this.SplitwiseContextMock.Expenses.Single(e => e.Id == transaction.SplitwiseTransaction.Value.Id);
            account = this.context.Accounts.GetEntity(account.Id);

            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(250, transaction.SplitwiseTransaction.Value.OwedByOthers);
            Assert.Equal(50, transaction.SplitwiseTransaction.Value.PersonalAmount);
            Assert.Equal(300, transaction.SplitwiseTransaction.Value.PaidAmount);
            Assert.Equal(-50, transaction.PersonalAmount);

            Assert.Equal(transaction.SplitwiseTransactionId.Value, expense.Id);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PaidAmount, expense.PaidAmount);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PersonalAmount, expense.PersonalAmount);
            Assert.Equal(transaction.Date, expense.Date.ToDateString());
            Assert.False(expense.IsDeleted);

            Assert.Equal(2, transaction.SplitDetails.Count);
            Assert.Contains(transaction.SplitDetails, sd => sd.SplitwiseUserId == 1 && sd.Amount == 100);
            Assert.Contains(transaction.SplitDetails, sd => sd.SplitwiseUserId == 2 && sd.Amount == 150);

            Assert.Equal(-300, account.CurrentBalance);
        }

        /// <summary>
        /// Tests method <see cref="ITransactionManager.CreateTransaction"/>.
        /// Verifies that a transaction with specified Splitwise splits is correctly created.
        /// </summary>
        [Fact]
        public void Test_CreateTransaction_SplitwiseSplitsAndPaymentRequests()
        {
            this.SplitwiseContextMock.GenerateUser(1, "Wouter", "van Acht");
            this.SplitwiseContextMock.GenerateUser(2, "Jeroen");

            var account = this.GenerateAccount();
            var splitwiseAccount = this.GenerateAccount(AccountType.Splitwise);
            var category = this.GenerateCategory();

            var input = new InputTransaction
            {
                AccountId = account.Id,
                Description = "Transaction",
                DateString = DateTime.Today.ToDateString(),
                Amount = -500,
                CategoryId = category.Id,
                ReceivingAccountId = Maybe<int>.None,
                NeedsConfirmation = false,
                PaymentRequests = new List<InputPaymentRequest>
                {
                    new InputPaymentRequest
                    {
                        Id = Maybe<int>.None,
                        Amount = 50,
                        Count = 2,
                        Name = "Group 1",
                    },
                    new InputPaymentRequest
                    {
                        Id = Maybe<int>.None,
                        Amount = 50,
                        Count = 1,
                        Name = "Person",
                    },
                },
                SplitwiseSplits = new List<InputSplitwiseSplit>
                {
                    new InputSplitwiseSplit
                    {
                        Amount = 100,
                        UserId = 1,
                    },
                    new InputSplitwiseSplit
                    {
                        Amount = 150,
                        UserId = 2,
                    },
                },
            };

            var transaction = this.TransactionManager.CreateTransaction(input);
            var expense =
                this.SplitwiseContextMock.Expenses.Single(e => e.Id == transaction.SplitwiseTransaction.Value.Id);
            account = this.AccountManager.GetAccount(account.Id);

            Assert.True(transaction.SplitwiseTransaction.IsSome);
            Assert.True(transaction.SplitwiseTransaction.Value.Imported);
            Assert.Equal(250, transaction.SplitwiseTransaction.Value.OwedByOthers);
            Assert.Equal(250, transaction.SplitwiseTransaction.Value.PersonalAmount);
            Assert.Equal(500, transaction.SplitwiseTransaction.Value.PaidAmount);
            Assert.Equal(-100, transaction.PersonalAmount);

            Assert.Equal(transaction.SplitwiseTransactionId.Value, expense.Id);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PaidAmount, expense.PaidAmount);
            Assert.Equal(transaction.SplitwiseTransaction.Value.PersonalAmount, expense.PersonalAmount);
            Assert.Equal(transaction.Date, expense.Date.ToDateString());
            Assert.False(expense.IsDeleted);

            Assert.Equal(-500, account.CurrentBalance);
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
                Type = TransactionType.Expense,
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
                Type = TransactionType.Expense,
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
                Type = TransactionType.Transfer,
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

        /// <summary>
        /// Tests the <see cref="ITransactionManager.DeleteTransaction"/> method. Verifies that the Splitwise
        /// transaction is removed when deleting a transaction.
        /// </summary>
        [Fact]
        public void DeleteTransaction_Splitwise()
        {
            this.SplitwiseContextMock.GenerateUser(1, "User1");
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount();
            var (splitwiseAccount, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var splits = this.context.GenerateSplitDetail(1, 7.5M).Singleton();
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.UtcNow.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                10,
                2.5M,
                true,
                splits);
            var transaction = this.context.Transactions
                .Add(splitwiseTransaction.ToTransaction(account, category))
                .Entity;
            this.SplitwiseContextMock.GenerateExpense(
                1,
                "Description",
                DateTime.UtcNow.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                10,
                2.5M,
                splits.Select(s => s.AsSplit()).ToList());

            this.SaveAndProcess();

            // Delete the transaction.
            this.TransactionManager.DeleteTransaction(transaction.Id);

            // Verify revert and removal
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single(a => a.Id == account.Id).CurrentBalance;
            var splitwiseBalance = this.context.Accounts.Single(a => a.Id == splitwiseAccount.Id).CurrentBalance;
            Assert.Equal(0, accountBalance);
            Assert.Equal(0, splitwiseBalance);
            splitwiseTransaction = this.context.SplitwiseTransactions.Single();
            Assert.True(splitwiseTransaction.IsDeleted);
            Wv8Assert.IsNone(this.context.Transactions.SingleOrNone());
            Wv8Assert.IsNone(this.SplitwiseContextMock.Expenses.SingleOrNone());
        }

        /// <summary>
        /// Tests the <see cref="ITransactionManager.DeleteTransaction"/> method. Verifies that the Splitwise
        /// transaction is removed when deleting a transaction.
        /// </summary>
        [Fact]
        public void DeleteTransaction_Splitwise_NotPaid()
        {
            var category = this.context.GenerateCategory();
            var (account, _) = this.context.GenerateAccount(AccountType.Splitwise);
            var splitwiseTransaction = this.context.GenerateSplitwiseTransaction(
                1,
                "Description",
                DateTime.UtcNow.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                0,
                10,
                true);
            var transaction = this.context.Transactions
                .Add(splitwiseTransaction.ToTransaction(account, category))
                .Entity;
            this.SplitwiseContextMock.GenerateExpense(
                1,
                "Description",
                DateTime.UtcNow.ToLocalDate(),
                false,
                DateTime.UtcNow.AddHours(-1),
                0,
                10);

            this.SaveAndProcess();

            // Delete the transaction.
            this.TransactionManager.DeleteTransaction(transaction.Id);

            // Verify revert and removal
            this.RefreshContext();
            var accountBalance = this.context.Accounts.Single().CurrentBalance;
            Assert.Equal(0, accountBalance);
            splitwiseTransaction = this.context.SplitwiseTransactions.Single();
            Assert.True(splitwiseTransaction.IsDeleted);
            Wv8Assert.IsNone(this.context.Transactions.SingleOrNone());
            Wv8Assert.IsNone(this.SplitwiseContextMock.Expenses.SingleOrNone());
        }

        #endregion DeleteTransaction

        #region UpdateTransactionCategory

        /// <summary>
        /// Tests the <see cref="ITransactionManager.UpdateTransactionCategory"/> method.
        /// Verifies the category is properly updated.
        /// </summary>
        [Fact]
        public void Test_UpdateTransactionCategory()
        {
            var (account, _) = this.context.GenerateAccount();
            var category1 = this.context.GenerateCategory();
            var category2 = this.context.GenerateCategory();

            var transaction = this.context.GenerateTransaction(account, category: category1);

            this.context.SaveChanges();

            this.TransactionManager.UpdateTransactionCategory(transaction.Id, category2.Id);

            this.RefreshContext();

            transaction = this.context.Transactions.GetEntity(transaction.Id);

            Assert.Equal(category2.Id, transaction.CategoryId.Value);
        }

        #endregion UpdateTransactionCategory

        #region FulfillPaymentRequest

        /// <summary>
        /// Test method for the <see cref="ITransactionManager.FulfillPaymentRequest"/> method.
        /// Tests that payment requests are correctly fulfilled and can not exceed requested amount.
        /// </summary>
        [Fact]
        public void Test_FulfillPaymentRequest()
        {
            var account = this.GenerateAccount();
            var category = this.GenerateCategory();
            // TODO: These should use generator methods so that id's are explicit
            var paymentRequests = new List<InputPaymentRequest>
            {
                new InputPaymentRequest
                {
                    Amount = 20,
                    Count = 5,
                    Name = "Group",
                },
                new InputPaymentRequest
                {
                    Amount = 30,
                    Count = 1,
                    Name = "Person",
                },
            };

            var transaction = this.GenerateTransaction(
                accountId: account.Id,
                type: TransactionType.Expense,
                date: DateTime.Today.ToLocalDate(),
                amount: -150,
                categoryId: category.Id,
                paymentRequests: paymentRequests);

            var paymentRequestGroup = transaction.PaymentRequests.Single(pr => pr.Count == 5);
            var paymentRequestPerson = transaction.PaymentRequests.Single(pr => pr.Count == 1);

            paymentRequestPerson = this.TransactionManager.FulfillPaymentRequest(paymentRequestPerson.Id);
            Assert.Equal(0, paymentRequestPerson.AmountDue);
            Assert.Equal(1, paymentRequestPerson.PaidCount);
            Assert.True(paymentRequestPerson.Complete);

            paymentRequestGroup = this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id);
            Assert.Equal(80, paymentRequestGroup.AmountDue);
            Assert.Equal(1, paymentRequestGroup.PaidCount);
            Assert.False(paymentRequestGroup.Complete);

            transaction = this.TransactionManager.GetTransaction(transaction.Id);
            Assert.Equal(-20, transaction.PersonalAmount);

            paymentRequestGroup = this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id);
            paymentRequestGroup = this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id);
            paymentRequestGroup = this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id);
            paymentRequestGroup = this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id);
            Assert.Equal(0, paymentRequestGroup.AmountDue);
            Assert.Equal(5, paymentRequestGroup.PaidCount);
            Assert.True(paymentRequestGroup.Complete);

            Assert.Throws<ValidationException>(() =>
                this.TransactionManager.FulfillPaymentRequest(paymentRequestPerson.Id));
            Assert.Throws<ValidationException>(() =>
                this.TransactionManager.FulfillPaymentRequest(paymentRequestGroup.Id));
        }

        #endregion FulfillPaymentRequest

        #region Helpers

        private Transaction UpdateTransaction(
            int id,
            int accountId,
            string description,
            string date,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId)
        {
            var input = new InputTransaction
            {
                AccountId = accountId,
                Description = description,
                DateString = date,
                Amount = amount,
                CategoryId = categoryId,
                ReceivingAccountId = receivingAccountId,
                PaymentRequests = new List<InputPaymentRequest>(),
                SplitwiseSplits = new List<InputSplitwiseSplit>(),
            };

            return this.TransactionManager.UpdateTransaction(id, input);
        }

        private Transaction CreateTransaction(
            int accountId,
            string description,
            string date,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            bool needsConfirmation)
        {
            var input = new InputTransaction
            {
                AccountId = accountId,
                Description = description,
                DateString = date,
                Amount = amount,
                CategoryId = categoryId,
                ReceivingAccountId = receivingAccountId,
                NeedsConfirmation = needsConfirmation,
                PaymentRequests = new List<InputPaymentRequest>(),
                SplitwiseSplits = new List<InputSplitwiseSplit>(),
            };

            return this.TransactionManager.CreateTransaction(input);
        }

        #endregion Helpers
    }
}