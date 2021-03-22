namespace PersonalFinance.Service.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Service endpoint for actions related to categories.
    /// </summary>
    [ApiController]
    [Route("api/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public TransactionController(ITransactionManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves a transaction based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        /// <returns>The transaction.</returns>
        [HttpGet("{id}")]
        public Transaction GetTransaction(int id)
        {
            return this.manager.GetTransaction(id);
        }

        /// <summary>
        /// Retrieves transactions from the database with a specified filter.
        /// </summary>
        /// <param name="type">Optionally, the transaction type filter.</param>
        /// <param name="accountId">Optionally, identifier of the account the transaction belongs to.</param>
        /// <param name="description">Optionally, the description filter.</param>
        /// <param name="categoryId">Optionally, identifier of the category the transaction belongs to.</param>
        /// <param name="startDate">Optionally, the start of the period on which to filter.</param>
        /// <param name="endDate">Optionally, the end of the period on which to filter.</param>
        /// <param name="skip">Specifies the number of items to be ignored, so the collection of returned items starts
        /// at the item after the last skipped one.</param>
        /// <param name="take">Specifies the number of items to be included into the returned collection.</param>
        /// <remarks>Note that both start and end date have to be filled to filter on period.</remarks>
        /// <returns>The list of filtered transactions.</returns>
        [HttpGet("filter")]
        public TransactionGroup GetTransactionsByFilter(
            [FromQuery] Maybe<TransactionType> type,
            [FromQuery] Maybe<int> accountId,
            [FromQuery] Maybe<string> description,
            [FromQuery] Maybe<int> categoryId,
            [FromQuery] Maybe<string> startDate,
            [FromQuery] Maybe<string> endDate,
            int skip,
            int take)
        {
            return this.manager.GetTransactionsByFilter(type, accountId, description, categoryId, startDate, endDate, skip, take);
        }

        /// <summary>
        /// Updates an transaction.
        /// </summary>
        /// <param name="id">The identifier of the to be updated transaction.</param>
        /// <param name="input">The input with the values for the to be updated transaction.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}")]
        public Transaction UpdateTransaction(int id, InputTransaction input)
        {
            return this.manager.UpdateTransaction(id, input);
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="input">The input with the values for the to be created transaction.</param>
        /// <returns>The created transaction.</returns>
        [HttpPost]
        public Transaction CreateTransaction(InputTransaction input)
        {
            return this.manager.CreateTransaction(input);
        }

        /// <summary>
        /// Updates the category of a transaction.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        /// <param name="categoryId">The category identifier.</param>
        [HttpPut("{id}/update-category")]
        public void UpdateTransactionCategory(int id, int categoryId)
        {
            this.manager.UpdateTransactionCategory(id, categoryId);
        }

        /// <summary>
        /// Confirms a transaction so that it can be processed.
        /// </summary>
        /// <param name="id">The identifier of the unconfirmed transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <returns>The confirmed transaction.</returns>
        [HttpPut("{id}/confirm")]
        public Transaction ConfirmTransaction(int id, string date, decimal amount)
        {
            return this.manager.ConfirmTransaction(id, date, amount);
        }

        /// <summary>
        /// Removes a transaction.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        [HttpDelete("{id}")]
        public void DeleteTransaction(int id)
        {
            this.manager.DeleteTransaction(id);
        }
    }
}