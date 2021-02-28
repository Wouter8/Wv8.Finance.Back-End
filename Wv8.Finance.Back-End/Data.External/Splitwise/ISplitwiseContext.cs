namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Data.External.Splitwise.Models;

    /// <summary>
    /// An interface for a context providing functionality to Splitwise.
    /// </summary>
    public interface ISplitwiseContext
    {
        /// <summary>
        /// Creates an expense in Splitwise.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="date">The date.</param>
        /// <param name="splits">The splits. This should also contain an entry for the user.</param>
        /// <returns>The created expense.</returns>
        public Expense CreateExpense(
            string description,
            LocalDate date,
            List<Split> splits);

        /// <summary>
        /// Gets all expenses which were updated/created after the specified timestamp.
        /// </summary>
        /// <param name="updatedAfter">The timestamp after which an expense must have been created/updated to be
        /// retrieved.</param>
        /// <returns>The list of expenses.</returns>
        public List<Expense> GetExpenses(DateTime updatedAfter);

        /// <summary>
        /// Get all users from the group of the user.
        /// </summary>
        /// <returns>A list of Splitwise users.</returns>
        public List<User> GetUsers();
    }
}