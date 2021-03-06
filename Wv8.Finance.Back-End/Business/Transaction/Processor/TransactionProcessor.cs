namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.External.Splitwise.Models;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class which handles transactions and recurring objects.
    /// </summary>
    public class TransactionProcessor : BaseManager
    {
        /// <summary>
        /// The splitwise context.
        /// </summary>
        private readonly ISplitwiseContext splitwiseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionProcessor"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public TransactionProcessor(Context context, ISplitwiseContext splitwiseContext)
            : base(context)
        {
            this.splitwiseContext = splitwiseContext;
        }

        /// <summary>
        /// Processes all entities that need to be processed.
        /// </summary>
        public void ProcessAll()
        {
            this.ConcurrentInvoke(() =>
            {
                // New daily balances are added to the list
                var newDailyBalances = this.ProcessTransactions();
                this.ProcessRecurringTransactions(newDailyBalances);

                this.Context.SaveChanges();
            });
        }

        #region Checks

        /// <summary>
        /// Processes a specific transaction if the transaction needs to be processed.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void ProcessIfNeeded(TransactionEntity transaction)
        {
            if (transaction.NeedsProcessing())
                this.Process(transaction);
        }

        /// <summary>
        /// Reverts the processing of a transaction if it was already processed.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        public void RevertIfProcessed(TransactionEntity transaction)
        {
            if (transaction.Processed)
                this.Revert(transaction);
        }

        /// <summary>
        /// Processes a specific recurring transaction if the recurring transaction has a start date which is before or
        /// equal to the current date.
        /// </summary>
        /// <param name="recurringTransaction">The recurring transaction.</param>
        public void ProcessIfNeeded(RecurringTransactionEntity recurringTransaction)
        {
            if (recurringTransaction.NeedsProcessing())
                this.Process(recurringTransaction);
        }

        #endregion Checks

        #region Single

        /// <summary>
        /// Processes a transaction. Meaning the value is added to the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(TransactionEntity transaction)
        {
            this.Process(transaction, new List<DailyBalanceEntity>());
        }

        /// <summary>
        /// Processes a transaction. Meaning the value is added to the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="addedDailyBalances">The daily balances that were already added. This is needed since the
        /// context does not contain already added entities, resulting in possible double daily balances which results
        /// in an error.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(
            TransactionEntity transaction, List<DailyBalanceEntity> addedDailyBalances)
        {
            transaction.VerifyEntitiesNotObsolete();

            var (historicalEntriesToEdit, newHistoricalEntry) =
                this.GetBalanceEntriesToEdit(transaction.AccountId, transaction.Date, addedDailyBalances);
            if (newHistoricalEntry.IsSome)
                addedDailyBalances.Add(newHistoricalEntry.Value);

            // If Splitwise splits are defined, but there is no linked Splitwise transaction yet, then create
            // the transaction in Splitwise.
            if (transaction.SplitDetails.Any() && !transaction.SplitwiseTransactionId.HasValue)
            {
                var splits = transaction.SplitDetails
                    .Select(sd => new Split
                        {
                            UserId = sd.SplitwiseUserId,
                            Amount = sd.Amount,
                        })
                    .ToList();

                var expense = this.splitwiseContext.CreateExpense(
                    transaction.Amount, transaction.Description, transaction.Date, splits);

                transaction.SplitwiseTransactionId = expense.Id;
                transaction.SplitwiseTransaction = expense.ToSplitwiseTransactionEntity();

                // The category is known at this point, so set imported to true.
                transaction.SplitwiseTransaction.Imported = true;
            }

            var personalAmount = transaction.PersonalAmount;

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    // Update budgets.
                    var budgets = this.Context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                        budget.Spent += Math.Abs(personalAmount);

                    // Update the Splitwise account balance if the transaction has a linked Splitwise transaction.
                    if (transaction.SplitwiseTransactionId.HasValue)
                        this.Process(transaction.SplitwiseTransaction, addedDailyBalances);

                    break;
                case TransactionType.Income:
                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    break;
                case TransactionType.Transfer:
                    var (receiverEntriesToEdit, newReceiverEntry) =
                        this.GetBalanceEntriesToEdit(
                            transaction.ReceivingAccountId.Value, transaction.Date, addedDailyBalances);
                    if (newReceiverEntry.IsSome)
                        addedDailyBalances.Add(newReceiverEntry.Value);

                    foreach (var entry in historicalEntriesToEdit)
                        entry.Balance -= transaction.Amount;
                    foreach (var entry in receiverEntriesToEdit)
                        entry.Balance += transaction.Amount;

                    break;
                default:
                    throw new InvalidOperationException("Unknown transaction type.");
            }

            transaction.Processed = true;
        }

        /// <summary>
        /// Reverses the processing of a transaction. Meaning the value is removed from the account, budgets and savings.
        /// This alters all historical values starting at the transaction date.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Revert(TransactionEntity transaction)
        {
            if (!transaction.Processed)
                throw new NotSupportedException("Transaction has not been processed.");

            var (historicalBalances, _) = this.GetBalanceEntriesToEdit(transaction.AccountId, transaction.Date);
            var personalAmount = transaction.PersonalAmount;

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance -= transaction.Amount;

                    // Update budgets.
                    var budgets = this.Context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                        budget.Spent -= Math.Abs(personalAmount);

                    // Update the Splitwise account balance if the transaction has a linked Splitwise transaction.
                    if (transaction.SplitwiseTransactionId.HasValue)
                    {
                        this.Revert(transaction.SplitwiseTransaction);

                        // If the transaction has splits, then it is defined in this application and the expense should
                        // be removed.
                        if (transaction.SplitDetails.Any())
                            this.splitwiseContext.DeleteExpense(transaction.SplitwiseTransactionId.Value);
                    }

                    break;
                case TransactionType.Income:
                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance -= transaction.Amount;

                    break;
                case TransactionType.Transfer:
                    var (receiverHistoricalBalances, _) =
                        this.GetBalanceEntriesToEdit(transaction.ReceivingAccountId.Value, transaction.Date);

                    foreach (var historicalBalance in historicalBalances)
                        historicalBalance.Balance += transaction.Amount;
                    foreach (var historicalBalance in receiverHistoricalBalances)
                        historicalBalance.Balance -= transaction.Amount;

                    break;
            }

            transaction.Processed = false;
        }

        /// <summary>
        /// Processes a recurring transaction. Meaning that instances get created.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(RecurringTransactionEntity transaction)
        {
            this.Process(transaction, new List<DailyBalanceEntity>());
        }

        /// <summary>
        /// Processes a recurring transaction. Meaning that instances get created.
        /// </summary>
        /// <param name="transaction">The recurring transaction.</param>
        /// <param name="addedDailyBalances">The daily balances that were already added. This is needed since the
        /// context does not contain already added entities, resulting in possible double daily balances which results
        /// in an error.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        private void Process(RecurringTransactionEntity transaction, List<DailyBalanceEntity> addedDailyBalances)
        {
            transaction.VerifyEntitiesNotObsolete();

            var instances = new List<TransactionEntity>();
            while (!transaction.Finished)
            {
                // Create transactions until a couple days in the future.
                if (transaction.NextOccurence > DateTime.Today.AddDays(7).ToLocalDate())
                    break;

                var instance = transaction.CreateOccurence();
                var isFuture = instance.Date > DateTime.Today.ToLocalDate();

                // Immediately process if transaction does not need to be confirmed.
                if (!instance.NeedsConfirmation && !isFuture)
                    this.Process(instance, addedDailyBalances);

                instances.Add(instance);
            }

            this.Context.Transactions.AddRange(instances);
        }

        /// <summary>
        /// Processes a Splitwise transaction. Meaning that the Splitwise account balances are updated if needed.
        /// </summary>
        /// <param name="transaction">The Splitwise transaction.</param>
        /// <param name="addedDailyBalances">The daily balances that were already added. This is needed since the
        /// context does not contain already added entities, resulting in possible double daily balances which results
        /// in an error.</param>
        private void Process(SplitwiseTransactionEntity transaction, List<DailyBalanceEntity> addedDailyBalances)
        {
            transaction.VerifyProcessable();

            var splitwiseAccount = this.Context.Accounts.GetSplitwiseEntity();
            var (historicalEntriesToEdit, newHistoricalEntry) =
                this.GetBalanceEntriesToEdit(splitwiseAccount.Id, transaction.Date, addedDailyBalances);
            if (newHistoricalEntry.IsSome)
                addedDailyBalances.Add(newHistoricalEntry.Value);
            var mutationAmount = transaction.GetSplitwiseAccountDifference();

            foreach (var entry in historicalEntriesToEdit)
                entry.Balance += mutationAmount;
        }

        /// <summary>
        /// Reverses the processing of a Splitwise transaction. Meaning that the Splitwise account balances are updated
        /// if needed.
        /// </summary>
        /// <param name="transaction">The Splitwise transaction.</param>
        private void Revert(SplitwiseTransactionEntity transaction)
        {
            transaction.VerifyProcessable();

            var splitwiseAccount = this.Context.Accounts.GetSplitwiseEntity();
            var (historicalEntriesToEdit, _) =
                this.GetBalanceEntriesToEdit(splitwiseAccount.Id, transaction.Date);

            var mutationAmount = transaction.GetSplitwiseAccountDifference();

            foreach (var entry in historicalEntriesToEdit)
                entry.Balance -= mutationAmount;
        }

        #endregion Single

        #region Bulk

        /// <summary>
        /// Processes the transactions that are in the past.
        /// </summary>
        /// <returns>The list of added daily balances.</returns>
        private List<DailyBalanceEntity> ProcessTransactions()
        {
            var today = LocalDate.FromDateTime(DateTime.Today);
            var transactionsToBeProcessed = this.Context.Transactions
                .IncludeAll()
                .Where(t => !t.Processed && t.Date <= today &&
                            (!t.NeedsConfirmation || (t.NeedsConfirmation && t.IsConfirmed.Value)))
                .ToList();

            // New daily balances are added to the list in the Process method.
            var newDailyBalances = new List<DailyBalanceEntity>();

            foreach (var transaction in transactionsToBeProcessed)
            {
                this.Process(transaction, newDailyBalances);
            }

            return newDailyBalances;
        }

        /// <summary>
        /// Processes the recurring transactions that have to be created.
        /// </summary>
        /// <param name="addedDailyBalances">The daily balances that were already added. This is needed since the
        /// context does not contain already added entities, resulting in possible double daily balances which results
        /// in an error.</param>
        private void ProcessRecurringTransactions(List<DailyBalanceEntity> addedDailyBalances)
        {
            var today = LocalDate.FromDateTime(DateTime.Today);
            var recurringTransactions = this.Context.RecurringTransactions
                .IncludeAll()
                .Where(rt => !rt.Finished && rt.StartDate <= today)
                .ToList();

            foreach (var recurringTransaction in recurringTransactions)
            {
                this.Process(recurringTransaction, addedDailyBalances);
            }
        }

        #endregion Bulk

        #region Helpers

        /// <summary>
        /// Gets the historical balance entries which should be edited based on a date. If the date has no entry,
        /// a new historical entry will be created and inserted in history.
        /// </summary>
        /// <param name="accountId">The account identifier for which to check the historical entries.</param>
        /// <param name="date">The date from which should be checked.</param>
        /// <param name="addedDailyBalances">The daily balances that were already added. This is needed since the
        /// context does not contain already added entities, resulting in possible double daily balances which results
        /// in an error.</param>
        /// <returns>A tuple containing the list of to be updated entities and optionally the newly created daily
        /// balance.</returns>
        private (List<DailyBalanceEntity>, Maybe<DailyBalanceEntity>) GetBalanceEntriesToEdit(
            int accountId, LocalDate date, Maybe<List<DailyBalanceEntity>> addedDailyBalances = default)
        {
            var balanceEntries = addedDailyBalances
                .ValueOrElse(new List<DailyBalanceEntity>())
                .Where(db => db.AccountId == accountId)
                .Concat(this.Context.DailyBalances
                    .Where(db => db.AccountId == accountId))
                .ToList();
            var balanceEntriesAfterDate = balanceEntries
                .Where(hb => hb.Date >= date)
                .OrderBy(hb => hb.Date)
                .ToList();

            // If date already has historical entry, just alter that and later entries.
            var balanceEntryOnSameDate = balanceEntries.SingleOrNone(hb => hb.Date == date);
            if (balanceEntryOnSameDate.IsSome)
                return (balanceEntriesAfterDate, Maybe<DailyBalanceEntity>.None);

            // Get last entity before date
            var lastEntry = balanceEntries
                .Where(hb => hb.Date < date)
                .OrderBy(hb => hb.Date)
                .LastOrNone();

            var newBalanceEntry = new DailyBalanceEntity
            {
                AccountId = accountId,
                Balance = lastEntry.Select(e => e.Balance).ValueOrElse(0),
                Date = date,
            };
            this.Context.DailyBalances.Add(newBalanceEntry);

            // Return all entries after the date + the new entry
            return (newBalanceEntry.Enumerate()
                    .Concat(balanceEntriesAfterDate)
                    .ToList(), newBalanceEntry);
        }

        #endregion Helpers
    }
}