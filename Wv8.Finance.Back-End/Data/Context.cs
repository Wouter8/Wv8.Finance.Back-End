namespace PersonalFinance.Data
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// The database context which provides read/write functionality to the database.
    /// </summary>
    public class Context : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Context"/> class with specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        public Context(DbContextOptions<Context> options)
            : base(options)
        {
        }

        /// <summary>
        /// The set of accounts.
        /// </summary>
        public DbSet<AccountEntity> Accounts { get; set; }

        /// <summary>
        /// The set of icons.
        /// </summary>
        public DbSet<IconEntity> Icons { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.BuildEntities();

            base.OnModelCreating(modelBuilder);
        }
    }
}