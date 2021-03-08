namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class providing helper methods in the form of extension methods to process transactions.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Gets a value indicating if the transaction needs to be processed.
        /// </summary>
        /// <param name="entity">The transaction.</param>
        /// <returns>A boolean indicating if the transaction needs to be processed.</returns>
        public static bool NeedsProcessing(this TransactionEntity entity)
        {
            return entity.Date <= LocalDate.FromDateTime(DateTime.Today) &&
                   // Is confirmed is always filled if needs confirmation is true.
                   // ReSharper disable once PossibleInvalidOperationException
                   (!entity.NeedsConfirmation || entity.IsConfirmed.Value);
        }

        /// <summary>
        /// Gets a value indicating if the recurring transaction needs to be processed.
        /// </summary>
        /// <param name="entity">The transaction.</param>
        /// <returns>A boolean indicating if the transaction needs to be processed.</returns>
        public static bool NeedsProcessing(this RecurringTransactionEntity entity)
        {
            return entity.StartDate <= LocalDate.FromDateTime(DateTime.Today);
        }

        /// <summary>
        /// Calculates and sets the next occurrence for a recurring transaction.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        public static void SetNextOccurrence(this RecurringTransactionEntity transaction)
        {
            var start = transaction.LastOccurence ?? transaction.StartDate;
            var next = LocalDate.MinIsoValue;
            switch (transaction.IntervalUnit)
            {
                case IntervalUnit.Days:
                    next = start.PlusDays(transaction.Interval);
                    break;
                case IntervalUnit.Weeks:
                    next = start.PlusWeeks(transaction.Interval);
                    break;
                case IntervalUnit.Months:
                    next = start.PlusMonths(transaction.Interval);
                    break;
                case IntervalUnit.Years:
                    next = start.PlusYears(transaction.Interval);
                    break;
            }

            if (!transaction.EndDate.HasValue || next <= transaction.EndDate)
            {
                transaction.NextOccurence = next;
            }
            else
            {
                transaction.NextOccurence = null;
                transaction.Finished = true;
            }
        }

        /// <summary>
        /// Creates a new occurence and calculates the next occurence for a recurring transaction.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <returns>The created transaction.</returns>
        /// <remarks>Note that the date is not validated in this method.</remarks>
        public static TransactionEntity CreateOccurence(this RecurringTransactionEntity transaction)
        {
            if (!transaction.NextOccurence.HasValue)
                throw new InvalidOperationException("Recurring transaction has no next occurence date set.");

            var instance = new TransactionEntity
            {
                AccountId = transaction.AccountId,
                Account = transaction.Account,
                Amount = transaction.Amount,
                CategoryId = transaction.CategoryId,
                Category = transaction.Category,
                Date = transaction.NextOccurence.Value,
                Description = transaction.Description,
                Processed = false,
                ReceivingAccountId = transaction.ReceivingAccountId,
                ReceivingAccount = transaction.ReceivingAccount,
                RecurringTransactionId = transaction.Id,
                RecurringTransaction = transaction,
                NeedsConfirmation = transaction.NeedsConfirmation,
                IsConfirmed = transaction.NeedsConfirmation ? false : (bool?)null,
                Type = transaction.Type,
                PaymentRequests = new List<PaymentRequestEntity>(),
                SplitDetails = transaction.SplitDetails.Select(sd =>
                    new SplitDetailEntity
                    {
                        Amount = sd.Amount,
                        SplitwiseUserId = sd.SplitwiseUserId,
                    }).ToList(),
            };
            transaction.LastOccurence = transaction.NextOccurence.Value;

            transaction.SetNextOccurrence();

            return instance;
        }

        /// <summary>
        /// Verifies that the entities linked to a transaction are not obsolete.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public static void VerifyEntitiesNotObsolete(
            this TransactionEntity transaction, ISplitwiseContext splitwiseContext)
        {
            if (transaction.Account == null)
                throw new ArgumentNullException(nameof(transaction.Account));
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(transaction.ReceivingAccount));
            if (transaction.CategoryId.HasValue && transaction.Category == null)
                throw new ArgumentNullException(nameof(transaction.Category));

            if (transaction.Account.IsObsolete)
                throw new IsObsoleteException($"Account is obsolete.");
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount.IsObsolete)
                throw new IsObsoleteException($"Receiver is obsolete.");
            if (transaction.CategoryId.HasValue && transaction.Category.IsObsolete)
                throw new IsObsoleteException($"Category is obsolete.");

            var currentSplitwiseUserIds = splitwiseContext.GetUsers().Select(u => u.Id).ToList();
            foreach (var splitDetail in transaction.SplitDetails)
            {
                if (!currentSplitwiseUserIds.Contains(splitDetail.SplitwiseUserId))
                    throw new IsObsoleteException("Splitwise user is obsolete.");
            }
        }

        /// <summary>
        /// Verifies that the entities linked to a recurring transaction are not obsolete.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public static void VerifyEntitiesNotObsolete(
            this RecurringTransactionEntity transaction, ISplitwiseContext splitwiseContext)
        {
            if (transaction.Account == null)
                throw new ArgumentNullException(nameof(transaction.Account));
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(transaction.ReceivingAccount));
            if (transaction.CategoryId.HasValue && transaction.Category == null)
                throw new ArgumentNullException(nameof(transaction.Category));

            if (transaction.Account.IsObsolete)
                throw new IsObsoleteException($"Account is obsolete.");
            if (transaction.ReceivingAccountId.HasValue && transaction.ReceivingAccount.IsObsolete)
                throw new IsObsoleteException($"Receiver is obsolete.");
            if (transaction.CategoryId.HasValue && transaction.Category.IsObsolete)
                throw new IsObsoleteException($"Category is obsolete.");

            var currentSplitwiseUserIds = splitwiseContext.GetUsers().Select(u => u.Id).ToList();
            foreach (var splitDetail in transaction.SplitDetails)
            {
                if (!currentSplitwiseUserIds.Contains(splitDetail.SplitwiseUserId))
                    throw new IsObsoleteException("Splitwise user is obsolete.");
            }
        }

        /// <summary>
        /// Verifies that a Splitwise transaction is processable. This is the case when the transaction has been
        /// imported and is not deleted.
        /// </summary>
        /// <param name="transaction">The entity to verify.</param>
        public static void VerifyProcessable(this SplitwiseTransactionEntity transaction)
        {
            if (!transaction.Imported)
            {
                throw new InvalidOperationException(
                    "This Splitwise transaction is not imported and should therefore not be processed.");
            }

            if (transaction.IsDeleted)
            {
                throw new InvalidOperationException(
                    "This Splitwise transaction is marked deleted and should therefore not be processed.");
            }
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
    }
}