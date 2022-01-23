namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Interface for the manager providing functionality related to recurring transactions.
    /// </summary>
    public interface IRecurringTransactionManager
    {
        /// <summary>
        /// Retrieves a recurring transaction based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction.</param>
        /// <returns>The recurring transaction.</returns>
        RecurringTransaction GetRecurringTransaction(int id);

        /// <summary>
        /// Retrieves recurring transactions from the database with a specified filter.
        /// </summary>
        /// <param name="type">Optionally, the recurring transaction type filter.</param>
        /// <param name="accountId">Optionally, identifier of the account the transaction belongs to.</param>
        /// <param name="categoryId">Optionally, identifier of the category the recurring transaction belongs to.</param>
        /// <param name="includeFinished">A value indicating if the finished recurring transactions should also be retrieved.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered recurring transactions.</returns>
        List<RecurringTransaction> GetRecurringTransactionsByFilter(
            Maybe<TransactionType> type,
            Maybe<int> accountId,
            Maybe<int> categoryId,
            bool includeFinished);

        /// <summary>
        /// Updates an recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction to be updated.</param>
        /// <param name="input">The input for the recurring transaction.</param>
        /// <param name="updateInstances">A value indicating if already created instances should be updated as well.</param>
        /// <returns>The updated recurring transaction.</returns>
        RecurringTransaction UpdateRecurringTransaction(int id, InputRecurringTransaction input, bool updateInstances);

        /// <summary>
        /// Creates a new recurring transaction.
        /// </summary>
        /// <param name="input">The input for the recurring transaction.</param>
        /// <returns>The created recurring transaction.</returns>
        RecurringTransaction CreateRecurringTransaction(InputRecurringTransaction input);

        /// <summary>
        /// Removes a recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction.</param>
        /// <param name="deleteInstances">If true, all instances derived from the recurring transaction are deleted.</param>
        void DeleteRecurringTransaction(int id, bool deleteInstances);
    }
}