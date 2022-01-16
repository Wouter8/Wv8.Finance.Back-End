namespace Business.UnitTest.Integration.Helpers
{
    using System;
    using System.Collections.Generic;
    using Business.UnitTest.Integration.Mocks;
    using NodaTime;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.External.Splitwise.Models;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods for a <see cref="Context"/> with which entities can be generated.
    /// </summary>
    public static class GenerationExtensions
    {
        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="type">The account type.</param>
        /// <param name="description">The description.</param>
        /// <param name="isDefault">A value indicating if the account is the default account.</param>
        /// <param name="isObsolete">A value indicating if the account is obsolete..</param>
        /// <param name="firstBalanceDate">The date on which the first balance entry should be added.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
        public static (AccountEntity, DailyBalanceEntity) GenerateAccount(
            this Context context,
            AccountType type = AccountType.Normal,
            string description = null,
            bool isDefault = false,
            bool isObsolete = false,
            LocalDate? firstBalanceDate = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            var account = context.Accounts.Add(new AccountEntity
            {
                Description = description ?? GetRandomString(),
                Type = type,
                IsDefault = isDefault,
                IsObsolete = isObsolete,
                Icon = new IconEntity
                {
                    Pack = iconPack ?? GetRandomString(3),
                    Name = iconName ?? GetRandomString(6),
                    Color = iconColor ?? GetRandomString(7),
                },
            }).Entity;

            var dailyBalance = context.DailyBalances.Add(new DailyBalanceEntity
            {
                Account = account,
                Balance = 0,
                Date = firstBalanceDate ?? DateTime.Now.ToLocalDate(),
            }).Entity;

            return (account, dailyBalance);
        }

        /// <summary>
        /// Creates a daily balance entity with specified values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="account">The account.</param>
        /// <param name="date">The date.</param>
        /// <param name="balance">The balance.</param>
        /// <returns>The created entity.</returns>
        public static DailyBalanceEntity GenerateDailyBalance(
            this Context context,
            AccountEntity account,
            LocalDate date,
            decimal balance)
        {
            return context.DailyBalances.Add(new DailyBalanceEntity
            {
                Account = account,
                Balance = balance,
                Date = date,
            }).Entity;
        }

        /// <summary>
        /// Creates a category with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="description">The description.</param>
        /// <param name="expectedMonthlyAmount">The expected monthly amount.</param>
        /// <param name="parentCategoryId">The identifier of the parent category.</param>
        /// <param name="isObsolete">A value indicating if the category is obsolete.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
        public static CategoryEntity GenerateCategory(
            this Context context,
            string description = null,
            decimal? expectedMonthlyAmount = null,
            int? parentCategoryId = null,
            bool isObsolete = false,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            return context.Categories.Add(new CategoryEntity
            {
                Description = description ?? GetRandomString(),
                IsObsolete = isObsolete,
                ParentCategoryId = parentCategoryId,
                ExpectedMonthlyAmount = expectedMonthlyAmount,
                Children = new List<CategoryEntity>(),
                Icon = new IconEntity
                {
                    Pack = iconPack ?? GetRandomString(6),
                    Name = iconName ?? GetRandomString(3),
                    Color = iconColor ?? GetRandomString(7),
                },
            }).Entity;
        }

        /// <summary>
        /// Creates a transaction with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="account">The account.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="category">The category.</param>
        /// <param name="receivingAccount">The receiving account.</param>
        /// <param name="recurringTransaction">The recurring transaction from which this is an instance.</param>
        /// <param name="splitwiseTransaction">The Splitwise transaction which is linked to this transaction.</param>
        /// <param name="paymentRequests">The payment requests that are linked to this transaction.</param>
        /// <param name="splitDetails">The split details which are linked to this transaction.</param>
        /// <param name="needsConfirmation">A value indicating if the transaction has to be confirmed.</param>
        /// <returns>The created transaction.</returns>
        public static TransactionEntity GenerateTransaction(
            this Context context,
            AccountEntity account,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? date = null,
            decimal? amount = null,
            CategoryEntity category = null,
            AccountEntity receivingAccount = null,
            RecurringTransactionEntity recurringTransaction = null,
            SplitwiseTransactionEntity splitwiseTransaction = null,
            List<PaymentRequestEntity> paymentRequests = null,
            List<SplitDetailEntity> splitDetails = null,
            bool needsConfirmation = false)
        {
            if ((type == TransactionType.Expense || type == TransactionType.Income) && category == null)
                throw new Exception("Specify a category for an income or expense transaction.");
            if (type == TransactionType.Transfer && receivingAccount == null)
                throw new Exception("Specify a receiving account for a transfer transaction.");

            return context.Transactions.Add(new TransactionEntity
            {
                Account = account,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? GetRandomString(),
                Date = date ?? DateTime.Now.ToLocalDate(),
                Category = category,
                ReceivingAccount = receivingAccount,
                NeedsConfirmation = needsConfirmation,
                PaymentRequests = paymentRequests ?? new List<PaymentRequestEntity>(),
                SplitDetails = splitDetails ?? new List<SplitDetailEntity>(),
                Processed = false,
                IsConfirmed = !needsConfirmation,
                Type = type,
                RecurringTransaction = recurringTransaction,
                SplitwiseTransaction = splitwiseTransaction,
            }).Entity;
        }

        /// <summary>
        /// Creates a recurring transaction with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="account">The account.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="startDate">The start date of the transaction.</param>
        /// <param name="endDate">The end date of the transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="category">The category.</param>
        /// <param name="receivingAccount">The receiving account.</param>
        /// <param name="needsConfirmation">A value indicating if the transaction has to be confirmed.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="intervalUnit">The interval unit.</param>
        /// <param name="paymentRequests">The payment requests that are linked to this transaction.</param>
        /// <param name="splitDetails">The split details which are linked to this transaction.</param>
        /// <returns>The created transaction.</returns>
        public static RecurringTransactionEntity GenerateRecurringTransaction(
            this Context context,
            AccountEntity account,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? startDate = null,
            LocalDate? endDate = null,
            decimal? amount = null,
            CategoryEntity category = null,
            AccountEntity receivingAccount = null,
            bool needsConfirmation = false,
            int interval = 1,
            IntervalUnit intervalUnit = IntervalUnit.Weeks,
            List<PaymentRequestEntity> paymentRequests = null,
            List<SplitDetailEntity> splitDetails = null)
        {
            if ((type == TransactionType.Expense || type == TransactionType.Income) && category == null)
                throw new Exception("Specify a category for an income or expense transaction.");
            if (type == TransactionType.Transfer && receivingAccount == null)
                throw new Exception("Specify a receiving account for a transfer transaction.");

            var start = startDate ?? DateTime.Now.ToLocalDate();

            var entity = new RecurringTransactionEntity
            {
                Account = account,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? GetRandomString(),
                StartDate = start,
                NextOccurence = start,
                EndDate = endDate,
                Category = category,
                ReceivingAccount = receivingAccount,
                NeedsConfirmation = needsConfirmation,
                Type = type,
                Interval = interval,
                IntervalUnit = intervalUnit,
                PaymentRequests = paymentRequests ?? new List<PaymentRequestEntity>(),
                SplitDetails = splitDetails ?? new List<SplitDetailEntity>(),
            };

            return context.RecurringTransactions.Add(entity).Entity;
        }

        /// <summary>
        /// Creates a Splitwise transaction with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="date">The date.</param>
        /// <param name="isDeleted">A value indicating if the expense is deleted.</param>
        /// <param name="updatedAt">The updated at timestamp.</param>
        /// <param name="paidAmount">The paid amount.</param>
        /// <param name="personalAmount">The personal amount.</param>
        /// <param name="imported">The imported value.</param>
        /// <param name="splits">The splits.</param>
        /// <returns>The created expense.</returns>
        public static SplitwiseTransactionEntity GenerateSplitwiseTransaction(
            this Context context,
            int id = 0,
            string description = null,
            LocalDate? date = null,
            bool isDeleted = false,
            DateTime? updatedAt = null,
            decimal paidAmount = 10,
            decimal personalAmount = 5,
            bool imported = false,
            List<SplitDetailEntity> splits = null)
        {
            var expense = new SplitwiseTransactionEntity
            {
                Id = id,
                Description = description ?? GetRandomString(),
                Date = date ?? DateTime.Today.ToLocalDate(),
                IsDeleted = isDeleted,
                UpdatedAt = updatedAt ?? DateTime.Now,
                PaidAmount = paidAmount,
                PersonalAmount = personalAmount,
                Imported = imported,
                SplitDetails = splits ?? new List<SplitDetailEntity>(),
            };

            context.SplitwiseTransactions.Add(expense);

            return expense;
        }

        /// <summary>
        /// Creates a split detail with specified or random values.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="splitwiseUserId">The Splitwise user id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="transactionId">The transaction id.</param>
        /// <param name="splitwiseTransactionId">The Splitwise transaction id.</param>
        /// <param name="splitwiseUserName">The Splitwise user name.</param>
        /// <returns>The created entity.</returns>
        public static SplitDetailEntity GenerateSplitDetail(
            this Context context,
            int splitwiseUserId,
            decimal amount = 10m,
            int? transactionId = null,
            int? splitwiseTransactionId = null,
            string splitwiseUserName = "User")
        {
            return context.SplitDetails.Add(new SplitDetailEntity
            {
                SplitwiseUserId = splitwiseUserId,
                SplitwiseUserName = splitwiseUserName,
                Amount = amount,
                TransactionId = transactionId,
                SplitwiseTransactionId = splitwiseTransactionId,
            }).Entity;
        }

        /// <summary>
        /// Creates an expense with specified, or random values.
        /// </summary>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="description">The description.</param>
        /// <param name="date">The date.</param>
        /// <param name="isDeleted">A value indicating if the expense is deleted.</param>
        /// <param name="updatedAt">The updated at timestamp.</param>
        /// <param name="paidAmount">The paid amount.</param>
        /// <param name="personalAmount">The personal amount.</param>
        /// <param name="splits">The splits.</param>
        /// <returns>The created expense.</returns>
        public static Expense GenerateExpense(
            this SplitwiseContextMock splitwiseContext,
            int id = 0,
            string description = null,
            LocalDate? date = null,
            bool isDeleted = false,
            DateTime? updatedAt = null,
            decimal paidAmount = 10,
            decimal personalAmount = 5,
            List<Split> splits = null)
        {
            var expense = new Expense
            {
                Id = id,
                Description = description ?? GetRandomString(),
                Date = date ?? DateTime.Today.ToLocalDate(),
                IsDeleted = isDeleted,
                UpdatedAt = updatedAt ?? DateTime.Now,
                PaidAmount = paidAmount,
                PersonalAmount = personalAmount,
                Splits = splits ?? new List<Split>(),
            };

            splitwiseContext.Expenses.Add(expense);

            return expense;
        }

        /// <summary>
        /// Creates a user with specified, or random values.
        /// </summary>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <returns>The created expense.</returns>
        public static User GenerateUser(
            this SplitwiseContextMock splitwiseContext,
            int id = 0,
            string firstName = null,
            string lastName = null)
        {
            var user = new User
            {
                Id = id,
                FirstName = firstName ?? GetRandomString(),
                LastName = lastName,
            };

            splitwiseContext.Users.Add(user);

            return user;
        }

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>The random string.</returns>
        private static string GetRandomString(int length = 16)
        {
            return Guid.NewGuid().ToString().Substring(0, length);
        }
    }
}
