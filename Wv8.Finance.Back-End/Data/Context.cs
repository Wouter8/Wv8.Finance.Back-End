namespace PersonalFinance.Data
{
    using System;
    using System.Linq;
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
        /// The set of historical accounts.
        /// </summary>
        public DbSet<AccountHistoryEntity> AccountHistory { get; set; }

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