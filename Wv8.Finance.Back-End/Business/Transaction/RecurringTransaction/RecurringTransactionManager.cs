namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common.DataTransfer;
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
            Maybe<int> categoryId)
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
                .OrderBy(t => t.NextOccurence)
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
            string endDate,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            int interval,
            IntervalUnit intervalUnit,
            bool needsConfirmation,
            bool updateInstances)
        {
            this.validator.Description(description);
            var startPeriod = this.validator.IsoString(startDate, nameof(startDate));
            var endPeriod = this.validator.IsoString(endDate, nameof(endDate));
            this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(interval);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.RecurringTransactions.GetEntity(id);
                this.validator.Type(entity.Type, amount, categoryId, receivingAccountId);

                var account = this.Context.Accounts.GetEntity(accountId, false);

                CategoryEntity category = null;
                if (categoryId.IsSome)
                {
                    category = this.Context.Categories.GetEntity(categoryId.Value, false);

                    if (entity.Type == TransactionType.Expense && category.Type != CategoryType.Expense)
                        throw new ValidationException($"Category \"{category.Description}\" is not an expense category.");
                    if (entity.Type == TransactionType.Income && category.Type != CategoryType.Income)
                        throw new ValidationException($"Category \"{category.Description}\" is not an income category.");
                }

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
                entity.EndDate = endPeriod;
                entity.Amount = amount;
                entity.CategoryId = categoryId.ToNullable();
                entity.Category = category;
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
                    entity.NextOccurence = null;
                    entity.Finished = false;
                }

                if (entity.StartDate <= DateTime.Today)
                    entity.ProcessRecurringTransaction(this.Context);

                this.Context.SaveChanges();

                return entity.AsRecurringTransaction();
            });
        }

        /// <inheritdoc />
        public RecurringTransaction CreateRecurringTransaction(
            int accountId,
            TransactionType type,
            string description,
            string startDate,
            string endDate,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            int interval,
            IntervalUnit intervalUnit,
            bool needsConfirmation)
        {
            this.validator.Description(description);
            var startPeriod = this.validator.IsoString(startDate, nameof(startDate));
            var endPeriod = this.validator.IsoString(endDate, nameof(endDate));
            this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(interval);

            return this.ConcurrentInvoke(() =>
            {
                this.validator.Type(type, amount, categoryId, receivingAccountId);

                var account = this.Context.Accounts.GetEntity(accountId, false);

                CategoryEntity category = null;
                if (categoryId.IsSome)
                {
                    category = this.Context.Categories.GetEntity(categoryId.Value, false);

                    if (type == TransactionType.Expense && category.Type != CategoryType.Expense)
                        throw new ValidationException($"Category \"{category.Description}\" is not an expense category.");
                    if (type == TransactionType.Income && category.Type != CategoryType.Income)
                        throw new ValidationException($"Category \"{category.Description}\" is not an income category.");
                }

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
                    EndDate = endPeriod,
                    AccountId = accountId,
                    Account = account,
                    CategoryId = categoryId.ToNullable(),
                    Category = category,
                    ReceivingAccountId = receivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount,
                    Interval = interval,
                    IntervalUnit = intervalUnit,
                    NeedsConfirmation = needsConfirmation,
                };

                if (entity.StartDate <= DateTime.Today)
                    entity.ProcessRecurringTransaction(this.Context);

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