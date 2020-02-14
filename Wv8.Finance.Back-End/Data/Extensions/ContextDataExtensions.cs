﻿namespace PersonalFinance.Data.Extensions
{
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

        #endregion Retrieve Extensions
    }
}