namespace Business.UnitTest.Helpers
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Models;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class containing extension methods for a <see cref="Context"/> with which entities can be generated.
    /// </summary>
    public static class GenerationExtensions
    {
        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="id">The transaction id.</param>
        /// <param name="type">The account type.</param>
        /// <param name="description">The description.</param>
        /// <param name="isDefault">A value indicating if the account is the default account.</param>
        /// <param name="isObsolete">A value indicating if the account is obsolete..</param>
        /// <param name="firstBalanceDate">The date on which the first balance entry should be added.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
        public static AccountEntity GenerateAccount(
            this Context context,
            int id = 0,
            AccountType type = AccountType.Normal,
            string description = null,
            bool isDefault = false,
            bool isObsolete = false,
            LocalDate? firstBalanceDate = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            var test = context.Accounts.Add(new AccountEntity
            {
                Id = id,
                Description = description ?? GetRandomString(),
                DailyBalances = new DailyBalanceEntity
                {
                    Balance = 0,
                    Date = firstBalanceDate ?? DateTime.Now.ToLocalDate(),
                }.Singleton(),
                Type = type,
                IsDefault = isDefault,
                IsObsolete = isObsolete,
                Icon = new IconEntity
                {
                    Pack = iconPack ?? GetRandomString(6),
                    Name = iconName ?? GetRandomString(3),
                    Color = iconColor ?? GetRandomString(7),
                },
            });

            return test.Entity;
        }

        /// <summary>
        /// Creates a category with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="id">The identifier of the category.</param>
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
            int id = 0,
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
                Id = id,
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
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="accountId">The identifier of the account.</param>
        /// <param name="id">The transaction id.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="receivingAccountId">The identifier of the receiving account.</param>
        /// <param name="recurringTransactionId">The identifier of the recurring transaction from which this is an
        /// instance.</param>
        /// <param name="needsConfirmation">A value indicating if the transaction has to be confirmed.</param>
        /// <returns>The created transaction.</returns>
        public static TransactionEntity GenerateTransaction(
            this Context context,
            int accountId,
            int id = 0,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? date = null,
            decimal? amount = null,
            int? categoryId = null,
            int? receivingAccountId = null,
            int? recurringTransactionId = null,
            bool needsConfirmation = false)
        {
            if ((type == TransactionType.Expense || type == TransactionType.Income) && !categoryId.HasValue)
                throw new Exception("Specify a category for an income or expense transaction.");
            if (type == TransactionType.Transfer && !receivingAccountId.HasValue)
                throw new Exception("Specify a receiving account for a transfer transaction.");

            return context.Transactions.Add(new TransactionEntity
            {
                Id = id,
                AccountId = accountId,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? GetRandomString(),
                Date = date ?? DateTime.Now.ToLocalDate(),
                CategoryId = categoryId,
                ReceivingAccountId = receivingAccountId,
                NeedsConfirmation = needsConfirmation,
                PaymentRequests = new List<PaymentRequestEntity>(),
                Processed = false,
                IsConfirmed = !needsConfirmation,
                Type = type,
                RecurringTransactionId = recurringTransactionId,
            }).Entity;
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