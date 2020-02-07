namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Service endpoint for actions related to categories.
    /// </summary>
    [ApiController]
    [Route("api/budgets")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public BudgetController(IBudgetManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves a budget based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        /// <returns>The budget.</returns>
        [HttpGet("{id}")]
        public Budget GetBudget(int id)
        {
            return this.manager.GetBudget(id);
        }

        /// <summary>
        /// Retrieves budgets from the database.
        /// </summary>
        /// <returns>The list of budgets.</returns>
        [HttpGet]
        public List<Budget> GetBudgets()
        {
            return this.manager.GetBudgets();
        }

        /// <summary>
        /// Retrieves budgets from the database with a specified filter.
        /// </summary>
        /// <param name="categoryId">Optionally, identifier of the category the budget has to track.</param>
        /// <param name="startDate">Optionally, the start of the period on which to filter.</param>
        /// <param name="endDate">Optionally, the end of the period on which to filter.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered budgets.</returns>
        [HttpGet("filter")]
        public List<Budget> GetBudgetsByFilter(Maybe<int> categoryId, Maybe<string> startDate, Maybe<string> endDate)
        {
            return this.manager.GetBudgetsByFilter(categoryId, startDate, endDate);
        }

        /// <summary>
        /// Updates an budget.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        /// <param name="description">The new description of the budget.</param>
        /// <param name="amount">The new amount of the budget.</param>
        /// <param name="startDate">The new start date of the budget.</param>
        /// <param name="endDate">The new end date of the budget.</param>
        /// <returns>The updated budget.</returns>
        [HttpPut("{id}")]
        public Budget UpdateBudget(int id, string description, decimal amount, string startDate, string endDate)
        {
            return this.manager.UpdateBudget(id, description, amount, startDate, endDate);
        }

        /// <summary>
        /// Creates a new budget.
        /// </summary>
        /// <param name="description">The description of the budget.</param>
        /// <param name="categoryId">The identifier of the category the budget has to track transactions for.</param>
        /// <param name="amount">The amount of the budget.</param>
        /// <param name="startDate">The start date of the budget.</param>
        /// <param name="endDate">The end date of the budget.</param>
        /// <returns>The created budget.</returns>
        [HttpPost]
        public Budget CreateBudget(string description, int categoryId, decimal amount, string startDate, string endDate)
        {
            return this.manager.CreateBudget(description, categoryId, amount, startDate, endDate);
        }

        /// <summary>
        /// Removes a budget.
        /// </summary>
        /// <param name="id">The identifier of the budget.</param>
        [HttpDelete("{id}")]
        public void DeleteBudget(int id)
        {
            this.manager.DeleteBudget(id);
        }
    }
}