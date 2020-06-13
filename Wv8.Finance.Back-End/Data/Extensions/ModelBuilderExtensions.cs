﻿namespace PersonalFinance.Data.Extensions
{
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods for the model builder of the database context.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Adds the required properties to the fields of all entities.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static void BuildEntities(this ModelBuilder builder)
        {
            builder.BuildIconEntity();
            builder.BuildAccountEntity();
            builder.BuildCategoryEntity();
            builder.BuildBudgetEntity();
            builder.BuildTransactionEntity();
            builder.BuildRecurringTransactionEntity();
            builder.BuildDailyBalanceEntity();
        }

        /// <summary>
        /// Adds the required properties to the fields of the icon entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildIconEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<IconEntity>();

            entity.ToTable("Icons");

            entity.Property(e => e.Pack).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Color).IsRequired();
        }

        /// <summary>
        /// Adds the required properties to the fields of the account entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildAccountEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<AccountEntity>();

            entity.ToTable("Accounts");

            entity.Property(e => e.Description).IsRequired();
        }

        /// <summary>
        /// Adds the required properties to the fields of the daily balance entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildDailyBalanceEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<DailyBalanceEntity>();

            entity.ToTable("DailyBalances");

            entity.HasKey(e => new { e.AccountId, e.Date });
            entity.Property(e => e.Balance).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the category entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildCategoryEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<CategoryEntity>();

            entity.ToTable("Categories");

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.ExpectedMonthlyAmount).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the budget entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildBudgetEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<BudgetEntity>();

            entity.ToTable("Budgets");

            entity.Property(e => e.Amount).HasPrecision(12, 2);
            entity.Property(e => e.Spent).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the transaction entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<TransactionEntity>();

            entity.ToTable("Transactions");

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the transaction entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildRecurringTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<RecurringTransactionEntity>();

            entity.ToTable("RecurringTransactions");

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(12, 2);
        }
    }
}