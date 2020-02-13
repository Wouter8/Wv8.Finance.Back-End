﻿namespace PersonalFinance.Business.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to budgets.
    /// </summary>
    public class BudgetManager : BaseManager, IBudgetManager
    {
        private readonly BudgetValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public BudgetManager(Context context)
            : base(context)
        {
            this.validator = new BudgetValidator();
        }

        /// <inheritdoc />
        public Budget GetBudget(int id)
        {
            return this.Context.Budgets
                .Include(b => b.Category)
                .ThenInclude(c => c.Icon)
                .SingleOrNone(b => b.Id == id)
                .ValueOrThrow(() => new DoesNotExistException($"Budget with identifier {id} does not exist."))
                .AsBudget();
        }

        /// <inheritdoc />
        public List<Budget> GetBudgets()
        {
            return this.Context.Budgets
                .Include(b => b.Category)
                .ThenInclude(c => c.Icon)
                .Where(b => !b.Category.IsObsolete)
                .Select(b => b.AsBudget())
                .ToList()
                .OrderBudgets();
        }

        /// <inheritdoc />
        public List<Budget> GetBudgetsByFilter(Maybe<int> categoryId, Maybe<string> startDate, Maybe<string> endDate)
        {
            var periodStart = startDate.Select(DateTime.Parse);
            var periodEnd = endDate.Select(DateTime.Parse);

            return this.Context.Budgets
                .Include(b => b.Category)
                .ThenInclude(c => c.Icon)
                .Include(b => b.Category)
                .ThenInclude(c => c.Children)
                .WhereIf(categoryId.IsSome, b => b.CategoryId == categoryId.Value)
                .WhereIf(
                    startDate.IsSome && endDate.IsSome,
                    b => (b.StartDate >= periodStart.Value && b.StartDate <= periodEnd.Value) ||
                                  (b.EndDate >= periodStart.Value && b.StartDate <= periodEnd.Value) ||
                                  (b.StartDate <= periodStart.Value && b.EndDate >= periodEnd.Value))
                .Where(b => !b.Category.IsObsolete)
                .Select(b => b.AsBudget())
                .ToList()
                .OrderBudgets();
        }

        /// <inheritdoc />
        public Budget UpdateBudget(int id, decimal amount, string startDate, string endDate)
        {
            var periodStart = DateTime.Parse(startDate);
            var periodEnd = DateTime.Parse(endDate);
            this.validator.Period(periodStart, periodEnd);
            this.validator.Amount(amount);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Budgets
                    .Include(b => b.Category)
                    .ThenInclude(c => c.Icon)
                    .SingleOrNone(b => b.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Budget with identifier {id} does not exist."));

                if (entity.Category.IsObsolete)
                    throw new ValidationException("The budget can not be updated since it is linked to an obsolete category.");

                // TODO: Set spent if dates changed
                entity.Amount = amount;
                entity.StartDate = periodStart;
                entity.EndDate = periodEnd;

                this.Context.SaveChanges();

                return entity.AsBudget();
            });
        }

        /// <inheritdoc />
        public Budget CreateBudget(int categoryId, decimal amount, string startDate, string endDate)
        {
            var periodStart = DateTime.Parse(startDate);
            var periodEnd = DateTime.Parse(endDate);
            this.validator.Period(periodStart, periodEnd);
            this.validator.Amount(amount);

            return this.ConcurrentInvoke(() =>
            {
                var category = this.Context.Categories
                    .Include(c => c.Icon)
                    .Include(c => c.Children)
                    .SingleOrNone(c => c.Id == categoryId)
                    .ValueOrThrow(() => new DoesNotExistException($"Category with identifier {categoryId} does not exist."));

                if (category.IsObsolete)
                    throw new ValidationException("Category is obsolete. No budgets can be created for obsolete categories.");

                if (category.Type != CategoryType.Expense)
                    throw new ValidationException("Budgets can only be created for expense categories.");

                var entity = new BudgetEntity
                {
                    CategoryId = categoryId,
                    Amount = amount,
                    Spent = 0, // TODO: Get all transactions and set sum
                    StartDate = periodStart,
                    EndDate = periodEnd,
                    Category = category,
                };

                this.Context.Budgets.Add(entity);
                this.Context.SaveChanges();

                return entity.AsBudget();
            });
        }

        /// <inheritdoc />
        public void DeleteBudget(int id)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Budgets
                    .SingleOrNone(b => b.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Budget with identifier {id} does not exist."));

                this.Context.Budgets.Remove(entity);

                this.Context.SaveChanges();
            });
        }
    }
}