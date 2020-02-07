namespace PersonalFinance.Business.Budget
{
    using System;
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Interface for the manager providing functionality related to budgets.
    /// </summary>
    public interface IBudgetManager
    {
        /// <summary>
        /// Retrieves a budget based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        /// <returns>The budget.</returns>
        Budget GetBudget(int id);

        /// <summary>
        /// Retrieves budgets from the database.
        /// </summary>
        /// <returns>The list of budgets.</returns>
        List<Budget> GetBudgets();

        /// <summary>
        /// Retrieves budgets from the database with a specified filter.
        /// </summary>
        /// <param name="categoryId">Optionally, identifier of the category the budget has to track.</param>
        /// <param name="startDate">Optionally, the start of the period on which to filter.</param>
        /// <param name="endDate">Optionally, the end of the period on which to filter.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered budgets.</returns>
        List<Budget> GetBudgetsByFilter(Maybe<int> categoryId, Maybe<string> startDate, Maybe<string> endDate);

        /// <summary>
        /// Updates an budget.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        /// <param name="description">The new description of the budget.</param>
        /// <param name="amount">The new amount of the budget.</param>
        /// <param name="startDate">The new start date of the budget.</param>
        /// <param name="endDate">The new end date of the budget.</param>
        /// <returns>The updated budget.</returns>
        Budget UpdateBudget(int id, string description, decimal amount, string startDate, string endDate);

        /// <summary>
        /// Creates a new budget.
        /// </summary>
        /// <param name="description">The description of the budget.</param>
        /// <param name="categoryId">The identifier of the category the budget has to track transactions for.</param>
        /// <param name="amount">The amount of the budget.</param>
        /// <param name="startDate">The start date of the budget.</param>
        /// <param name="endDate">The end date of the budget.</param>
        /// <returns>The created budget.</returns>
        Budget CreateBudget(string description, int categoryId, decimal amount, string startDate, string endDate);

        /// <summary>
        /// Removes a budget.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        void DeleteBudget(int id);
    }
}