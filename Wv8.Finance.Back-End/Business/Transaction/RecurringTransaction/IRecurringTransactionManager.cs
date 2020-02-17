﻿namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;
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
        /// <param name="categoryId">Optionally, identifier of the category the recurring transaction belongs to.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered recurring transactions.</returns>
        List<RecurringTransaction> GetRecurringTransactionsByFilter(Maybe<TransactionType> type, Maybe<int> categoryId);

        /// <summary>
        /// Updates an recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction to be updated.</param>
        /// <param name="accountId">The new identifier of the account this recurring transaction belongs to.</param>
        /// <param name="description">The new description of the recurring transaction.</param>
        /// <param name="startDate">The new start date of the recurring transaction.</param>
        /// <param name="endDate">The new end date of the recurring transaction.</param>
        /// <param name="amount">The new amount of the recurring transaction.</param>
        /// <param name="categoryId">The new identifier of the category the recurring transaction belongs to.</param>
        /// <param name="receivingAccountId">The new identifier of the receiving account.</param>
        /// <returns>The updated recurring transaction.</returns>
        RecurringTransaction UpdateRecurringTransaction(int id, int accountId, string description, string startDate, string endDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId);

        /// <summary>
        /// Creates a new recurring transaction.
        /// </summary>
        /// <param name="accountId">The identifier of the account this recurring transaction belongs to.</param>
        /// <param name="type">The type of the recurring transaction.</param>
        /// <param name="description">The description of the recurring transaction.</param>
        /// <param name="startDate">The start date of the recurring transaction.</param>
        /// <param name="endDate">The end date of the recurring transaction.</param>
        /// <param name="amount">The amount of the recurring transaction.</param>
        /// <param name="categoryId">The identifier of the category the recurring transaction belongs to.</param>
        /// <param name="receivingAccountId">The identifier of the receiving account.</param>
        /// <returns>The created recurring transaction.</returns>
        RecurringTransaction CreateRecurringTransaction(int accountId, TransactionType type, string description, string startDate, string endDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId);

        /// <summary>
        /// Removes a recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction.</param>
        /// <param name="deleteInstances">If true, all instances derived from the recurring transaction are deleted.</param>
        void DeleteRecurringTransaction(int id, bool deleteInstances);
    }
}