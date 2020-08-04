namespace PersonalFinance.Business.Transaction
{
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
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
        TransactionGroup GetTransactionsByFilter(Maybe<TransactionType> type, Maybe<int> accountId, Maybe<string> description, Maybe<int> categoryId, Maybe<string> startDate, Maybe<string> endDate, int skip, int take);

        /// <summary>
        /// Updates an transaction.
        /// </summary>
        /// <param name="input">The input with the values for the to be updated transaction.</param>
        /// <returns>The updated transaction.</returns>
        Transaction UpdateTransaction(EditTransaction input);

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="input">The input with the values for the to be created transaction.</param>
        /// <returns>The created transaction.</returns>
        Transaction CreateTransaction(InputTransaction input);

        /// <summary>
        /// Confirms a transaction so that it can be processed.
        /// </summary>
        /// <param name="id">The identifier of the unconfirmed transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount of the transaction.</param>
        /// <returns>The confirmed transaction.</returns>
        Transaction ConfirmTransaction(int id, string date, decimal amount);

        /// <summary>
        /// Removes a transaction.
        /// </summary>
        /// <param name="id">The identifier of the transaction.</param>
        void DeleteTransaction(int id);

        /// <summary>
        /// Fulfills a payment request once. This means that a payment request which contains multiple requests
        /// (<see cref="PaymentRequest.Count"/> > 1) has to be fulfilled multiple times.
        /// This method essentially increments the <see cref="PaymentRequest.PaidCount"/> with 1.
        /// </summary>
        /// <param name="id">The identifier of the payment request.</param>
        /// <returns>The updated payment request.</returns>
        PaymentRequest FulfillPaymentRequest(int id);

        /// <summary>
        /// Reverts the fulfillment of a payment request. This essentially means decreases the
        /// <see cref="PaymentRequest.PaidCount"/> by 1.
        /// </summary>
        /// <param name="id">The identifier of the payment request.</param>
        /// <returns>The updated payment request.</returns>
        PaymentRequest RevertPaymentPaymentRequest(int id);
    }
}