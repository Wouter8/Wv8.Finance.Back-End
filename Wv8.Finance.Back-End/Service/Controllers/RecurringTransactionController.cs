﻿namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Service endpoint for actions related to categories.
    /// </summary>
    [ApiController]
    [Route("api/transactions/recurring")]
    public class RecurringTransactionController : ControllerBase
    {
        private readonly IRecurringTransactionManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTransactionController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public RecurringTransactionController(IRecurringTransactionManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves a recurring transaction based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction.</param>
        /// <returns>The recurring transaction.</returns>
        [HttpGet("{id}")]
        public RecurringTransaction GetRecurringTransaction(int id)
        {
            return this.manager.GetRecurringTransaction(id);
        }

        /// <summary>
        /// Retrieves recurring transactions from the database with a specified filter.
        /// </summary>
        /// <param name="type">Optionally, the recurring transaction type filter.</param>
        /// <param name="accountId">Optionally, identifier of the account the transaction belongs to.</param>
        /// <param name="categoryId">Optionally, identifier of the category the recurring transaction belongs to.</param>
        /// <param name="includeFinished">A value indicating if the finished recurring transactions should also be retrieved.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered recurring transactions.</returns>
        [HttpGet("filter")]
        public List<RecurringTransaction> GetRecurringTransactionsByFilter(
            [FromQuery] Maybe<TransactionType> type,
            [FromQuery] Maybe<int> accountId,
            [FromQuery] Maybe<int> categoryId,
            bool includeFinished)
        {
            return this.manager.GetRecurringTransactionsByFilter(
                type,
                accountId,
                categoryId,
                includeFinished);
        }

        /// <summary>
        /// Updates an recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction to be updated.</param>
        /// <param name="input">The input for the recurring transaction.</param>
        /// <param name="updateInstances">A value indicating if already created instances should be updated as well.</param>
        /// <returns>The updated recurring transaction.</returns>
        [HttpPut("{id}")]
        public RecurringTransaction UpdateRecurringTransaction(
            int id,
            [FromBody] InputRecurringTransaction input,
            [FromQuery] bool updateInstances)
        {
            return this.manager.UpdateRecurringTransaction(
                id,
                input,
                updateInstances);
        }

        /// <summary>
        /// Creates a new recurring transaction.
        /// </summary>
        /// <param name="input">The input for the recurring transaction.</param>
        /// <returns>The created recurring transaction.</returns>
        [HttpPost]
        public RecurringTransaction CreateRecurringTransaction(InputRecurringTransaction input)
        {
            return this.manager.CreateRecurringTransaction(input);
        }

        /// <summary>
        /// Removes a recurring transaction.
        /// </summary>
        /// <param name="id">The identifier of the recurring transaction.</param>
        /// <param name="deleteInstances">If true, all instances derived from the recurring transaction are deleted.</param>
        [HttpDelete("{id}")]
        public void DeleteRecurringTransaction(int id, bool deleteInstances)
        {
            this.manager.DeleteRecurringTransaction(id, deleteInstances);
        }
    }
}