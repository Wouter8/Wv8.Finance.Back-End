﻿namespace PersonalFinance.Data
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.History;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// The database context which provides read/write functionality to the database.
    /// </summary>
    public class Context : DbContext, IHistoricalContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class with specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
            this.CreationTime = HistoryExtensions.GetUniqueDateTime();
        }

        /// <summary>
        /// The set of accounts.
        /// </summary>
        public DbSet<AccountEntity> Accounts { get; set; }

        /// <summary>
        /// The set of daily balances.
        /// </summary>
        public DbSet<DailyBalanceEntity> DailyBalances { get; set; }

        /// <summary>
        /// The set of categories.
        /// </summary>
        public DbSet<CategoryEntity> Categories { get; set; }

        /// <summary>
        /// The set of budgets.
        /// </summary>
        public DbSet<BudgetEntity> Budgets { get; set; }

        /// <summary>
        /// The set of transactions.
        /// </summary>
        public DbSet<TransactionEntity> Transactions { get; set; }

        /// <summary>
        /// The set of transactions.
        /// </summary>
        public DbSet<RecurringTransactionEntity> RecurringTransactions { get; set; }

        /// <summary>
        /// The set of icons.
        /// </summary>
        public DbSet<IconEntity> Icons { get; set; }

        /// <summary>
        /// The set of payment requests.
        /// </summary>
        public DbSet<PaymentRequestEntity> PaymentRequests { get; set; }

        /// <summary>
        /// The set of imported Splitwise transactions.
        /// </summary>
        public DbSet<SplitwiseTransactionEntity> SplitwiseTransactions { get; set; }

        /// <summary>
        /// The set of synchronization times. This should only contain a single entry.
        /// </summary>
        public DbSet<SynchronizationTimesEntity> SynchronizationTimes { get; set; }

        /// <summary>
        /// The set of split details.
        /// </summary>
        public DbSet<SplitDetailEntity> SplitDetails { get; set; }

        /// <inheritdoc/>
        public DateTime CreationTime { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.BuildEntities();

            base.OnModelCreating(modelBuilder);
        }
    }
}