namespace PersonalFinance.Business.Budget
{
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The validator for all fields related to budgets.
    /// </summary>
    public class BudgetValidator : BaseValidator
    {
        /// <summary>
        /// Validates the amount of a budget.
        /// </summary>
        /// <param name="amount">input.</param>
        public void Amount(decimal amount)
        {
            if (amount <= 0)
                throw new ValidationException("The amount of the budget has to be greater than zero.");
        }
    }
}