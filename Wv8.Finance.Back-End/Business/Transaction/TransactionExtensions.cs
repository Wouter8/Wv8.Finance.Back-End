namespace PersonalFinance.Business.Transaction
{
    using System.Linq;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods related to transactions.
    /// </summary>
    public static class TransactionExtensions
    {
        /// <summary>
        /// Get the amount of the transaction this is personally due. This can be different from the amount on the
        /// transaction when that amount contains an amount paid by others. These are stored in payment requests. The
        /// sum of the payment requests are subtracted from the amount on the transaction to get the personal amount.
        /// </summary>
        /// <param name="entity">The transaction entity.</param>
        /// <returns>The personal amount of the transaction.</returns>
        public static decimal GetPersonalAmount(this TransactionEntity entity)
        {
            return entity.Amount + entity.PaymentRequests.Sum(pr => pr.Count * pr.Amount);
        }
    }
}