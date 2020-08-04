namespace PersonalFinance.Business.Budget
{
    using System;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data.Models;

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
                Amount = entity.Amount,
                Spent = entity.Spent,
                StartDate = entity.StartDate.ToDateString(),
                EndDate = entity.EndDate.ToDateString(),
                CategoryId = entity.CategoryId,
                Category = entity.Category.AsCategory(),
            };
        }
    }
}