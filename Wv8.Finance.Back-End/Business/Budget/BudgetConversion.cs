namespace PersonalFinance.Business.Budget
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class BudgetConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static Budget AsBudget(this BudgetEntity entity)
        {
            if (entity.Category == null)
                throw new ArgumentNullException(nameof(entity.Category));

            return new Budget
            {
                Id = entity.Id,
                Description = entity.Description,
                Amount = entity.Amount,
                Spent = entity.Spent,
                StartDate = entity.StartDate.ToString("O"),
                EndDate = entity.EndDate.ToString("O"),
                CategoryId = entity.CategoryId,
                Category = entity.Category.AsCategory(),
            };
        }
    }
}