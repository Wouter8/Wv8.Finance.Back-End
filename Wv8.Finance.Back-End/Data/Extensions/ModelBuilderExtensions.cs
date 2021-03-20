namespace PersonalFinance.Data.Extensions
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.Enums;
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
            builder.BuildBaseTransactionEntity();
            builder.BuildTransactionEntity();
            builder.BuildRecurringTransactionEntity();
            builder.BuildDailyBalanceEntity();
            builder.BuildPaymentRequestEntity();
            builder.BuildSplitwiseTransactionEntity();
            builder.BuildSplitDetailEntity();
            builder.BuildSynchronizationTimesEntity();
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
            entity.Property(e => e.Type).HasDefaultValue(AccountType.Normal);
            entity.Property(e => e.CurrentBalance).HasPrecision(12, 2)
                .HasComputedColumnSql("GetCurrentBalance([Id])");
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
        private static void BuildBaseTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<BaseTransactionEntity>();

            entity.ToTable("BaseTransactions");

            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the transaction entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<TransactionEntity>();

            entity.ToTable("Transactions");
        }

        /// <summary>
        /// Adds the required properties to the fields of the transaction entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildRecurringTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<RecurringTransactionEntity>();

            entity.ToTable("RecurringTransactions");
        }

        /// <summary>
        /// Adds the required properties to the fields of the payment request entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildPaymentRequestEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<PaymentRequestEntity>();

            entity.ToTable("PaymentRequests");

            entity.Property(pr => pr.Name).IsRequired();
            entity.Property(pr => pr.Amount).HasPrecision(12, 2);

            builder.Entity<BaseTransactionEntity>()
                .HasMany(t => t.PaymentRequests)
                .WithOne()
                .HasForeignKey(pr => pr.TransactionId);
        }

        /// <summary>
        /// Adds the required properties to the fields of the Splitwise transaction entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildSplitwiseTransactionEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<SplitwiseTransactionEntity>();

            entity.ToTable("SplitwiseTransactions");

            entity.Property(st => st.Id).ValueGeneratedNever(); // Use id from Splitwise
            entity.Property(st => st.Description).IsRequired();
            entity.Property(st => st.PersonalAmount).HasPrecision(12, 2);
            entity.Property(st => st.PaidAmount).HasPrecision(12, 2);
        }

        /// <summary>
        /// Adds the required properties to the fields of the split detail entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildSplitDetailEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<SplitDetailEntity>();

            entity.ToTable("SplitDetails");

            entity
                .HasIndex(sd => new { sd.SplitwiseTransactionId, sd.SplitwiseUserId })
                .IsUnique();
            entity
                .HasIndex(sd => new { sd.TransactionId, sd.SplitwiseUserId })
                .IsUnique();
            entity.Property(sd => sd.Amount).HasPrecision(12, 2);

            builder.Entity<BaseTransactionEntity>()
                .HasMany(t => t.SplitDetails)
                .WithOne()
                .HasForeignKey(sd => sd.TransactionId);

            builder.Entity<SplitwiseTransactionEntity>()
                .HasMany(t => t.SplitDetails)
                .WithOne()
                .HasForeignKey(sd => sd.SplitwiseTransactionId);
        }

        /// <summary>
        /// Adds the required properties to the fields of the synchronization times entity.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private static void BuildSynchronizationTimesEntity(this ModelBuilder builder)
        {
            var entity = builder.Entity<SynchronizationTimesEntity>();

            entity.ToTable("SynchronizationTimes");

            entity.HasData(new SynchronizationTimesEntity { Id = 1, SplitwiseLastRun = DateTime.MinValue });
        }
    }
}