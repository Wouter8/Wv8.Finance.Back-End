namespace PersonalFinance.Data.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
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
                .Include(a => a.Icon);
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
        /// <returns>The account.</returns>
        public static AccountEntity GetEntity(this DbSet<AccountEntity> set, int id)
        {
            return set
                .IncludeAll()
                .SingleOrNone(a => a.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Account with identifier {id} does not exist."));
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
        /// <returns>The category.</returns>
        public static CategoryEntity GetEntity(this DbSet<CategoryEntity> set, int id)
        {
            return set
                .IncludeAll()
                .SingleOrNone(c => c.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {id} does not exist."));
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
        /// Retrieves a list of budgets based on some filters.
        /// </summary>
        /// <param name="set">The database set.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="date">The date.</param>
        /// <returns>The list of budgets.</returns>
        public static List<BudgetEntity> GetBudgets(
            this DbSet<BudgetEntity> set,
            int categoryId,
            DateTime date)
        {
            return set
                .IncludeAll()
                .Where(b => b.CategoryId == categoryId || b.Category.Children.Select(c => c.Id).Contains(categoryId))
                .Where(b => b.StartDate <= date && b.EndDate >= date)
                .ToList();
        }

        #endregion Retrieve Extensions
    }
}