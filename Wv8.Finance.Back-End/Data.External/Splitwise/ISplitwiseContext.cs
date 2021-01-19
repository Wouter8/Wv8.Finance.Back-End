namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Collections.Generic;
    using PersonalFinance.Data.External.Splitwise.DataTransfer;

    /// <summary>
    /// An interface for a context providing functionality to Splitwise.
    /// </summary>
    public interface ISplitwiseContext
    {
        /// <summary>
        /// Gets all expenses which were updated/created after the specified timestamp.
        /// </summary>
        /// <param name="updatedAfter">The timestamp after which an expense must have been created/updated to be
        /// retrieved.</param>
        /// <returns>The list of expenses.</returns>
        public List<Expense> GetExpenses(DateTime updatedAfter);
    }
}