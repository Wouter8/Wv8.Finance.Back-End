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
