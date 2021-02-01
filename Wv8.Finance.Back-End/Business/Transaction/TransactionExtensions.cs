namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods related to transactions.
    /// </summary>
    public static class TransactionExtensions
    {
        /// <summary>
        /// Processes the transaction if it needs to be processed.
        /// </summary>
        /// <param name="entity">The transaction entity.</param>
        /// <param name="context">The database context.</param>
        public static void ProcessIfNeeded(this TransactionEntity entity, Context context)
        {
            if (entity.NeedsProcessing())
                entity.ProcessTransaction(context);
        }

        /// <summary>
        /// Reverts the processing of a transaction if it was processed.
        /// </summary>
        /// <param name="entity">The transaction entity.</param>
        /// <param name="context">The database context.</param>
        public static void RevertIfProcessed(this TransactionEntity entity, Context context)
        {
            if (entity.Processed)
                entity.RevertProcessedTransaction(context);
        }

        /// <summary>
        /// Get the amount of the transaction that is personally due. This can be different from the amount on the
        /// transaction when that amount contains an amount paid for others or paid by others. These differences are
        /// stored in the linked Splitwise transaction or payment request.
        /// </summary>
        /// <param name="entity">The transaction entity.</param>
        /// <returns>The personal amount of the transaction.</returns>
        public static decimal GetPersonalAmount(this TransactionEntity entity)
        {
            // When I paid for others, then subtract the amount paid for others.
            // When someone else paid for me, then add that share to the personal amount.
            var splitwiseMutation = entity.SplitwiseTransactionId.HasValue
                ? entity.SplitwiseTransaction.OwedToOthers - entity.SplitwiseTransaction.OwedByOthers
                : 0;

            return entity.Amount
                   + entity.PaymentRequests.Sum(pr => pr.Count * pr.Amount)
                   + splitwiseMutation;
        }

        /// <summary>
        /// Gets the amount with which the Splitwise account should be updated. This can either be negative
        /// (in the case that something is owed to others) positive (when someone else owes something to me).
        /// </summary>
        /// <param name="entity">The transaction entity.</param>
        /// <returns>The mutation for the Splitwise account.</returns>
        public static decimal GetSplitwiseAccountDifference(this SplitwiseTransactionEntity entity)
        {
            return -entity.OwedToOthers + entity.OwedByOthers;
        }

        /// <summary>
        /// Gets a value indicating if the transaction needs to be processed.
        /// </summary>
        /// <param name="entity">The transaction.</param>
        /// <returns>A boolean indicating if the transaction needs to be processed.</returns>
        private static bool NeedsProcessing(this TransactionEntity entity)
        {
            return entity.Date <= LocalDate.FromDateTime(DateTime.Today) &&
                   // Is confirmed is always filled if needs confirmation is true.
                   // ReSharper disable once PossibleInvalidOperationException
                   (!entity.NeedsConfirmation || entity.IsConfirmed.Value);
        }
    }
}
