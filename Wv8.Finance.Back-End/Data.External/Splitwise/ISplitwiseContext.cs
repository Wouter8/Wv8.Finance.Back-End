namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using PersonalFinance.Data.External.Splitwise.Models;

    /// <summary>
    /// An interface for a context providing functionality to Splitwise.
    /// </summary>
    public interface ISplitwiseContext
    {
        /// <summary>
        /// A method which returns whether Splitwise integration is enabled.
        /// </summary>
        /// <returns><c>true</c> if the integration is enabled, <c>false</c> otherwise.</returns>
        public bool IntegrationEnabled();

        /// <summary>
        /// Creates an expense in Splitwise.
        /// </summary>
        /// <param name="totalAmount">The total amount of the transaction.</param>
        /// <param name="description">The description.</param>
        /// <param name="date">The date.</param>
        /// <param name="splits">The splits. This should also contain an entry for the user.</param>
        /// <returns>The created expense.</returns>
        public Expense CreateExpense(
            decimal totalAmount,
            string description,
            LocalDate date,
            List<Split> splits);

        /// <summary>
        /// Deletes an expense in Splitwise.
        /// </summary>
        /// <param name="id">The identifier of the expense in Splitwise.</param>
        public void DeleteExpense(long id);

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
