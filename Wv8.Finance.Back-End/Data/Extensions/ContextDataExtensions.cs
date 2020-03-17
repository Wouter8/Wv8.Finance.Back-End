namespace PersonalFinance.Data.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data.History;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// A class containing extension methods for the context which help with retrieving data.
    /// </summary>
    public static class ContextDataExtensions
    {
        #region Query Extensions

        /// <summary>
        /// Generates a query with all includes.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <returns>The base query.</returns>
        public static IQueryable<AccountEntity> IncludeAll(this DbSet<AccountEntity> set)
        {
            return set
                .Include(a => a.Icon)
                .Include(a => a.History);
        }

        /// <summary>
        /// Generates a query with all includes.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <returns>The base query.</returns>
        public static IQueryable<BudgetEntity> IncludeAll(this DbSet<BudgetEntity> set)
        {
            return set
                .Include(b => b.Category)
                .ThenInclude(c => c.Icon)
                .Include(b => b.Category)
                .ThenInclude(c => c.ParentCategory)
                .ThenInclude(c => c.Icon)
                .Include(b => b.Category)
                .ThenInclude(c => c.Children)
                .ThenInclude(c => c.Icon);
        }

        /// <summary>
        /// Generates a query with all includes.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <returns>The base query.</returns>
        public static IQueryable<CategoryEntity> IncludeAll(this DbSet<CategoryEntity> set)
        {
            return set
                .Include(c => c.Icon)
                .Include(c => c.ParentCategory)
                .ThenInclude(c => c.Icon)
                .Include(c => c.Children)
                .ThenInclude(c => c.Icon);
        }

        /// <summary>
        /// Generates a query with all includes.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <returns>The base query.</returns>
        public static IQueryable<TransactionEntity> IncludeAll(this DbSet<TransactionEntity> set)
        {
            return set
                .Include(t => t.Account)
                .ThenInclude(t => t.Icon)
                .Include(t => t.Account)
                .ThenInclude(t => t.History)
                .Include(t => t.ReceivingAccount)
                .ThenInclude(t => t.Icon)
                .Include(t => t.ReceivingAccount)
                .ThenInclude(t => t.History)
                .Include(t => t.Category)
                .ThenInclude(c => c.Icon)
                .Include(t => t.Category)
                .ThenInclude(c => c.ParentCategory)
                .ThenInclude(c => c.Icon)
                .Include(t => t.Category)
                .ThenInclude(c => c.Children)
                .ThenInclude(c => c.Icon);
        }

        /// <summary>
        /// Generates a query with all includes.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <returns>The base query.</returns>
        public static IQueryable<RecurringTransactionEntity> IncludeAll(this DbSet<RecurringTransactionEntity> set)
        {
            return set
                .Include(t => t.Account)
                .ThenInclude(t => t.Icon)
                .Include(t => t.Account)
                .ThenInclude(t => t.History)
                .Include(t => t.ReceivingAccount)
                .ThenInclude(t => t.Icon)
                .Include(t => t.ReceivingAccount)
                .ThenInclude(t => t.History)
                .Include(t => t.Category)
                .ThenInclude(c => c.Icon)
                .Include(t => t.Category)
                .ThenInclude(c => c.ParentCategory)
                .ThenInclude(c => c.Icon)
                .Include(t => t.Category)
                .ThenInclude(c => c.Children)
                .ThenInclude(c => c.Icon);
        }

        #endregion Query Extensions

        #region Retrieve Extensions

        /// <summary>
        /// Retrieves a account entity.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="id">The identifier of the account to be retrieved..</param>
        /// <param name="allowObsolete">A value indicating if the retrieved entity can be obsolete.</param>
        /// <returns>The account.</returns>
        public static AccountEntity GetEntity(this DbSet<AccountEntity> set, int id, bool allowObsolete = true)
        {
            var entity = set
                .IncludeAll()
                .SingleOrNone(a => a.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Account with identifier {id} does not exist."));

            if (!allowObsolete && entity.IsObsolete)
                throw new IsObsoleteException($"Account \"{entity.Description}\" is obsolete.");

            return entity;
        }

        /// <summary>
        /// Retrieves a budget entity.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="id">The identifier of the budget to be retrieved..</param>
        /// <returns>The budget.</returns>
        public static BudgetEntity GetEntity(this DbSet<BudgetEntity> set, int id)
        {
            return set
                .IncludeAll()
                .SingleOrNone(a => a.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Budget with identifier {id} does not exist."));
        }

        /// <summary>
        /// Retrieves a category entity.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="id">The identifier of the category to be retrieved..</param>
        /// <param name="allowObsolete">A value indicating if the retrieved entity can be obsolete.</param>
        /// <returns>The category.</returns>
        public static CategoryEntity GetEntity(this DbSet<CategoryEntity> set, int id, bool allowObsolete = true)
        {
            var entity = set
                .IncludeAll()
                .SingleOrNone(a => a.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {id} does not exist."));

            if (!allowObsolete && entity.IsObsolete)
                throw new IsObsoleteException($"Category \"{entity.Description}\" is obsolete.");

            return entity;
        }

        /// <summary>
        /// Retrieves a transaction entity.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="id">The identifier of the transaction to be retrieved..</param>
        /// <returns>The transaction.</returns>
        public static TransactionEntity GetEntity(this DbSet<TransactionEntity> set, int id)
        {
            return set
                .IncludeAll()
                .SingleOrNone(c => c.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Transaction with identifier {id} does not exist."));
        }

        /// <summary>
        /// Retrieves a transaction entity.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="id">The identifier of the transaction to be retrieved..</param>
        /// <returns>The transaction.</returns>
        public static RecurringTransactionEntity GetEntity(this DbSet<RecurringTransactionEntity> set, int id)
        {
            return set
                .IncludeAll()
                .SingleOrNone(c => c.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Recurring transaction with identifier {id} does not exist."));
        }

        /// <summary>
        /// Retrieves a list of budgets based on some filters.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="date">The date.</param>
        /// <returns>The list of budgets.</returns>
        public static List<BudgetEntity> GetBudgets(
            this DbSet<BudgetEntity> set,
            int categoryId,
            LocalDate date)
        {
            return set
                .IncludeAll()
                .Where(b => b.CategoryId == categoryId || b.Category.Children.Select(c => c.Id).Contains(categoryId))
                .Where(b => b.StartDate <= date && b.EndDate >= date)
                .ToList();
        }

        /// <summary>
        /// Retrieves a list of transactions based on some filters.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="start">The start date of the period to search for.</param>
        /// <param name="end">The end date of the period to search for.</param>
        /// <returns>The list of transactions.</returns>
        public static List<TransactionEntity> GetTransactions(this DbSet<TransactionEntity> set, int categoryId, LocalDate start, LocalDate end)
        {
            return set
                .IncludeAll()
                .Where(t => t.CategoryId == categoryId || (t.CategoryId.HasValue && t.Category.ParentCategoryId == categoryId))
                .Where(t => start <= t.Date && end >= t.Date)
                .ToList();
        }

        /// <summary>
        /// Retrieves a list of transactions based on some filters.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="recurringTransactionId">The recurring transaction identifier.</param>
        /// <returns>The list of transactions.</returns>
        public static List<TransactionEntity> GetTransactionsFromRecurring(this DbSet<TransactionEntity> set, int recurringTransactionId)
        {
            return set
                .IncludeAll()
                .Where(t => t.RecurringTransactionId == recurringTransactionId)
                .ToList();
        }

        #endregion Retrieve Extensions
    }
}