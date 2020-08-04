namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to recurring transactions.
    /// </summary>
    public class RecurringTransactionManager : BaseManager, IRecurringTransactionManager
    {
        /// <summary>
        /// The validator for input.
        /// <remarks>Using the transaction validator since most fields are the same between recurring and normal transactions.</remarks>
        /// </summary>
        private readonly TransactionValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTransactionManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public RecurringTransactionManager(Context context)
            : base(context)
        {
            this.validator = new TransactionValidator();
        }

        /// <inheritdoc />
        public RecurringTransaction GetRecurringTransaction(int id)
        {
            return this.Context.RecurringTransactions.GetEntity(id).AsRecurringTransaction();
        }

        /// <inheritdoc />
        public List<RecurringTransaction> GetRecurringTransactionsByFilter(
            Maybe<TransactionType> type,
            Maybe<int> accountId,
            Maybe<int> categoryId,
            bool includeFinished)
        {
            return this.Context.RecurringTransactions
                .IncludeAll()
                .WhereIf(type.IsSome, t => t.Type == type.Value)
                .WhereIf(accountId.IsSome, t => t.AccountId == accountId.Value || t.ReceivingAccountId == accountId.Value)
                .WhereIf(
                    categoryId.IsSome,
                    t => t.CategoryId.HasValue && (t.CategoryId.Value == categoryId.Value ||
                                                   (t.Category.ParentCategoryId.HasValue &&
                                                    t.Category.ParentCategoryId.Value == categoryId.Value)))
                .WhereIf(!includeFinished, rt => !rt.Finished)
                .OrderByDescending(t => t.StartDate)
                .ThenByDescending(t => t.EndDate)
                .ToList()
                .Select(t => t.AsRecurringTransaction())
                .ToList();
        }

        /// <inheritdoc />
        public RecurringTransaction UpdateRecurringTransaction(
            int id,
            int accountId,
            string description,
            string startDate,
            Maybe<string> endDate,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            int interval,
            IntervalUnit intervalUnit,
            bool needsConfirmation,
            bool updateInstances)
        {
            this.validator.Description(description);
            var startPeriod = this.validator.DateString(startDate, nameof(startDate));
            var endPeriod = endDate.Select(d => this.validator.DateString(d, nameof(endDate)));
            if (endPeriod.IsSome)
                this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(interval);
            var type = this.GetTransactionType(categoryId, receivingAccountId, amount);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.RecurringTransactions.GetEntity(id);
                if (type != entity.Type) // TODO: Add test for this case
                    throw new ValidationException("Changing the type of transaction is not possible.");

                if (!updateInstances && startPeriod != entity.StartDate)
                    throw new ValidationException($"Updating the start date without updating already created instances is not supported.");

                var account = this.Context.Accounts.GetEntity(accountId, false);

                var category = categoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                AccountEntity receivingAccount = null;
                if (receivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(receivingAccountId.Value, false);

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");
                }

                entity.AccountId = accountId;
                entity.Account = account;
                entity.Description = description;
                entity.StartDate = startPeriod;
                entity.EndDate = endPeriod.ToNullable();
                entity.Amount = amount;
                entity.CategoryId = categoryId.ToNullable();
                entity.Category = category.ToNullIfNone();
                entity.ReceivingAccountId = receivingAccountId.ToNullable();
                entity.ReceivingAccount = receivingAccount;
                entity.NeedsConfirmation = needsConfirmation;
                entity.Interval = interval;
                entity.IntervalUnit = intervalUnit;

                if (updateInstances)
                {
                    var instances = this.Context.Transactions.GetTransactionsFromRecurring(entity.Id);
                    foreach (var instance in instances)
                    {
                        // Although always in the past, a transaction might not be processed because it still has to be confirmed.
                        if (instance.Processed)
                            instance.RevertProcessedTransaction(this.Context);
                        this.Context.Remove(instance);
                    }

                    entity.NextOccurence = startPeriod;
                    entity.Finished = false;
                }

                if (entity.StartDate <= LocalDate.FromDateTime(DateTime.Today))
                    entity.ProcessRecurringTransaction(this.Context);

                this.Context.SaveChanges();

                return entity.AsRecurringTransaction();
            });
        }

        /// <inheritdoc />
        public RecurringTransaction CreateRecurringTransaction(
            int accountId,
            string description,
            string startDate,
            Maybe<string> endDate,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            int interval,
            IntervalUnit intervalUnit,
            bool needsConfirmation)
        {
            this.validator.Description(description);
            var startPeriod = this.validator.DateString(startDate, nameof(startDate));
            var endPeriod = endDate.Select(d => this.validator.DateString(d, nameof(endDate)));
            if (endPeriod.IsSome)
                this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(interval);
            var type = this.GetTransactionType(categoryId, receivingAccountId, amount);

            return this.ConcurrentInvoke(() =>
            {
                var account = this.Context.Accounts.GetEntity(accountId, false);

                var category = categoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                AccountEntity receivingAccount = null;
                if (receivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(receivingAccountId.Value, false);

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");
                }

                var entity = new RecurringTransactionEntity
                {
                    Description = description,
                    Type = type,
                    Amount = amount,
                    StartDate = startPeriod,
                    EndDate = endPeriod.ToNullable(),
                    AccountId = accountId,
                    Account = account,
                    CategoryId = categoryId.ToNullable(),
                    Category = category.ToNullIfNone(),
                    ReceivingAccountId = receivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount,
                    Interval = interval,
                    IntervalUnit = intervalUnit,
                    NeedsConfirmation = needsConfirmation,
                    NextOccurence = startPeriod,
                };

                if (entity.StartDate <= LocalDate.FromDateTime(DateTime.Today))
                    entity.ProcessRecurringTransaction(this.Context);

                this.Context.RecurringTransactions.Add(entity);
                this.Context.SaveChanges();

                return entity.AsRecurringTransaction();
            });
        }

        /// <inheritdoc />
        public void DeleteRecurringTransaction(int id, bool deleteInstances)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.RecurringTransactions.GetEntity(id);

                var instances = this.Context.Transactions.GetTransactionsFromRecurring(entity.Id);
                foreach (var instance in instances)
                {
                    if (deleteInstances)
                    {
                        // Although always in the past, a transaction might not be processed because it still has to be confirmed.
                        if (instance.Processed)
                            instance.RevertProcessedTransaction(this.Context);
                        this.Context.Remove(instance);
                    }
                    else
                    {
                        instance.RecurringTransactionId = null;
                    }
                }

                this.Context.Remove(entity);

                this.Context.SaveChanges();
            });
        }
    }
}