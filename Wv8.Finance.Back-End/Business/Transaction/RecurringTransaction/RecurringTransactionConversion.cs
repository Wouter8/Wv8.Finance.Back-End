namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System;
    using System.Linq;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class RecurringTransactionConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static RecurringTransaction AsRecurringTransaction(this RecurringTransactionEntity entity)
        {
            if (entity.CategoryId.HasValue && entity.Category == null)
                throw new ArgumentNullException(nameof(entity.Category));
            if (entity.Account == null)
                throw new ArgumentNullException(nameof(entity.Account));
            if (entity.ReceivingAccountId.HasValue && entity.ReceivingAccount == null)
                throw new ArgumentNullException(nameof(entity.ReceivingAccount));

            return new RecurringTransaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Amount = entity.Amount,
                StartDate = entity.StartDate.ToDateString(),
                EndDate = entity.EndDate.ToDateString(),
                Type = entity.Type,
                CategoryId = entity.CategoryId.ToMaybe(),
                Category = entity.Category.ToMaybe().Select(c => c.AsCategory()),
                AccountId = entity.AccountId,
                Account = entity.Account.AsAccount(),
                ReceivingAccountId = entity.ReceivingAccountId.ToMaybe(),
                ReceivingAccount = entity.ReceivingAccount.ToMaybe().Select(a => a.AsAccount()),
                IntervalUnit = entity.IntervalUnit,
                Interval = entity.Interval,
                NextOccurence = entity.NextOccurence.ToMaybe().Select(dt => dt.ToDateString()),
                Finished = entity.Finished,
                NeedsConfirmation = entity.NeedsConfirmation,
                SplitDetails = entity.SplitDetails.Select(sd => sd.AsSplitDetail()).ToList(),
                PaymentRequests = entity.PaymentRequests.Select(pr => pr.AsPaymentRequest()).ToList(),
                PersonalAmount = entity.PersonalAmount,
            };
        }
    }
}