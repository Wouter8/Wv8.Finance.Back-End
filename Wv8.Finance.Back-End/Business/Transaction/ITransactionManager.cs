namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Interface for the manager providing functionality related to transactions.
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        /// Retrieves a transaction based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        /// <returns>The transaction.</returns>
        Transaction GetTransaction(int id);

        /// <summary>
        /// Retrieves transactions from the database with a specified filter.
        /// </summary>
        /// <param name="type">Optionally, the transaction type filter.</param>
        /// <param name="categoryId">Optionally, identifier of the category the transaction belongs to.</param>
        /// <param name="startDate">Optionally, the start of the period on which to filter.</param>
        /// <param name="endDate">Optionally, the end of the period on which to filter.</param>
        /// <param name="skip">Specifies the number of items to be ignored, so the collection of returned items starts
        /// at the item after the last skipped one.</param>
        /// <param name="take">Specifies the number of items to be included into the returned collection.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered transactions.</returns>
        TransactionGroup GetTransactionsByFilter(Maybe<TransactionType> type, Maybe<int> categoryId, Maybe<string> startDate, Maybe<string> endDate, int skip, int take);

        /// <summary>
        /// Updates an transaction.
        /// </summary>
        /// <param name="description">The new description of the transaction.</param>
        /// <param name="date">The new date of the transaction.</param>
        /// <param name="amount">The new amount of the transaction.</param>
        /// <param name="categoryId">The new identifier of the category the transaction belongs to.</param>
        /// <returns>The updated transaction.</returns>
        Transaction UpdateTransaction(string description, string date, decimal amount, Maybe<int> categoryId);

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <param name="categoryId">The identifier of the category the transaction belongs to.</param>
        /// <returns>The created transaction.</returns>
        Transaction CreateTransaction(TransactionType type, string description, string date, decimal amount, Maybe<int> categoryId);

        /// <summary>
        /// Removes a transaction.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        void DeleteTransaction(int id);
    }
}