namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Comparers;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class TransactionConversion
    {
        /// <summary>
        /// Converts a payment request input to a payment request entity.
        /// </summary>
        /// <param name="paymentRequest">The payment request input.</param>
        /// <returns>The created entity.</returns>
        public static PaymentRequestEntity ToPaymentRequestEntity(this InputPaymentRequest paymentRequest)
        {
            return new PaymentRequestEntity
            {
                Amount = paymentRequest.Amount,
                Name = paymentRequest.Name,
                Count = paymentRequest.Count,
            };
        }

        /// <summary>
        /// Converts a split input to a split detail entity.
        /// </summary>
        /// <param name="split">The inputted split.</param>
        /// <returns>The created entity.</returns>
        public static SplitDetailEntity ToSplitDetailEntity(this InputSplitwiseSplit split)
        {
            return new SplitDetailEntity
            {
                SplitwiseUserId = split.UserId,
                Amount = split.Amount,
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="includePaymentRequests"><c>true</c> if the payment requests of the transaction need to be
        /// serialized, <c>false</c> otherwise.</param>
        /// <returns>The data transfer object.</returns>
        public static Transaction AsTransaction(this TransactionEntity entity, bool includePaymentRequests = true)
        {
            if (entity.CategoryId.HasValue && entity.Category == null)
                throw new ArgumentNullException(nameof(entity.Category));
            if (entity.Account == null)
                throw new ArgumentNullException(nameof(entity.Account));
            if (entity.ReceivingAccountId.HasValue && entity.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(entity.ReceivingAccount));
            if (entity.PaymentRequests == null)
                throw new ArgumentNullException(nameof(entity.PaymentRequests));
            if (entity.SplitwiseTransactionId.HasValue && entity.SplitwiseTransaction == null)
                throw new ArgumentNullException(nameof(entity.SplitwiseTransaction));

            return new Transaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Amount = entity.Amount,
                Date = entity.Date.ToDateString(),
                Type = entity.Type,
                CategoryId = entity.CategoryId.ToMaybe(),
                Category = entity.Category.ToMaybe().Select(c => c.AsCategory()),
                AccountId = entity.AccountId,
                Account = entity.Account.AsAccount(),
                ReceivingAccountId = entity.ReceivingAccountId.ToMaybe(),
                ReceivingAccount = entity.ReceivingAccount.ToMaybe().Select(a => a.AsAccount()),
                Processed = entity.Processed,
                RecurringTransactionId = entity.RecurringTransactionId.ToMaybe(),
                RecurringTransaction = entity.RecurringTransaction.ToMaybe().Select(t => t.AsRecurringTransaction()),
                NeedsConfirmation = entity.NeedsConfirmation,
                IsConfirmed = entity.IsConfirmed.ToMaybe(),
                PaymentRequests = includePaymentRequests
                    ? entity.PaymentRequests.Select(pr => pr.AsPaymentRequest()).ToList()
                    : new List<PaymentRequest>(),
                PersonalAmount = entity.PersonalAmount,
                SplitwiseTransactionId = entity.SplitwiseTransactionId.ToMaybe(),
                SplitwiseTransaction = entity.SplitwiseTransaction.ToMaybe().Select(st => st.AsSplitwiseTransaction()),
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static PaymentRequest AsPaymentRequest(this PaymentRequestEntity entity)
        {
            return new PaymentRequest
            {
                Id = entity.Id,
                Name = entity.Name,
                Amount = entity.Amount,
                Count = entity.Count,
                PaidCount = entity.PaidCount,
                TransactionId = entity.TransactionId,
                AmountDue = (entity.Count - entity.PaidCount) * entity.Amount,
                Complete = entity.PaidCount == entity.Count,
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entities">The entity.</param>
        /// <param name="totalSearchResults">The total search results retrieved. Might not be the same as the
        /// length of the list because of pagination parameters.</param>
        /// <returns>The data transfer object.</returns>
        public static TransactionGroup AsTransactionGroup(this List<TransactionEntity> entities, int totalSearchResults)
        {
            var transactions = entities.Select(e => e.AsTransaction()).ToList();
            var transactionsWithCategory = transactions.Where(t => t.CategoryId.IsSome).ToList();

            var categories = transactionsWithCategory
                .Select(t => t.Category.Value)
                .Distinct(new CategoryComparer())
                .ToDictionary(c => c.Id);

            return new TransactionGroup
            {
                TotalSearchResults = totalSearchResults,
                TotalSum = transactions.Sum(t => t.Amount),
                Transactions = transactions,
                // Not using .ToLookup() as this will need a special JSON-converter.
                SumPerCategory = transactionsWithCategory
                    .GroupBy(t => t.Category.Value.Id, t => t)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                TransactionsPerCategory = transactionsWithCategory
                    .GroupBy(t => t.Category.Value.Id, t => t)
                    .ToDictionary(g => g.Key, g => g.ToList()),
                TransactionsPerType = transactions
                    .GroupBy(t => t.Type)
                    .ToDictionary(g => g.Key, g => g.ToList()),
                Categories = categories,
            };
        }
    }
}