namespace PersonalFinance.Data.Models
{
    using System;

    /// <summary>
    /// An entity representing a budget. The amount of the budget is the goal sum of all
    /// transactions of the category in the period of the budget.
    /// </summary>
    public class BudgetEntity
    {
        /// <summary>
        /// The identifier of this budget.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The amount of this budget.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The date from which transactions in the category are tracked.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The date till which transactions in the category are tracked.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The amount currently spent.
        /// </summary>
        public decimal Spent { get; set; }

        /// <summary>
        /// The identifier of the category this budget tracks transactions for.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// The category of this budget.
        /// </summary>
        public CategoryEntity Category { get; set; }
    }
}