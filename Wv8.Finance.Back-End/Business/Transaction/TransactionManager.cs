namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to transactions.
    /// </summary>
    public class TransactionManager : BaseManager, ITransactionManager
    {
        private readonly TransactionValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TransactionManager(Context context)
            : base(context)
        {
            this.validator = new TransactionValidator();
        }

        /// <inheritdoc />
        public Transaction GetTransaction(int id)
        {
            return this.Context.Transactions.GetEntity(id).AsTransaction();
        }

        /// <inheritdoc />
        public TransactionGroup GetTransactionsByFilter(
            Maybe<TransactionType> type,
            Maybe<int> accountId,
            Maybe<string> description,
            Maybe<int> categoryId,
            Maybe<string> startDate,
            Maybe<string> endDate,
            int skip,
            int take)
        {
            description.Select(d => description = d.Trim());
            var startPeriod = startDate.Select(d => this.validator.DateString(d, nameof(startDate)));
            var endPeriod = endDate.Select(d => this.validator.DateString(d, nameof(endDate)));
            this.validator.Pagination(skip, take);
            this.validator.Period(startPeriod, endPeriod, true);

            var query = this.Context.Transactions
                .IncludeAll()
                .WhereIf(type.IsSome, t => t.Type == type.Value)
                .WhereIf(accountId.IsSome, t => t.AccountId == accountId.Value || t.ReceivingAccountId == accountId.Value)
                .WhereIf(
                    categoryId.IsSome,
                    t => t.CategoryId.HasValue && (t.CategoryId.Value == categoryId.Value ||
                                                   (t.Category.ParentCategoryId.HasValue &&
                                                    t.Category.ParentCategoryId.Value == categoryId.Value)))
                .WhereIf(startPeriod.IsSome, t => startPeriod.Value <= t.Date && endPeriod.Value >= t.Date)
                .WhereIf(
                    description.IsSome,
                    t => EF.Functions.Like(t.Description, $"%{description.Value}%") ||
                         EF.Functions.Like(t.Account.Description, $"%{description.Value}%") ||
                         (t.CategoryId.HasValue && EF.Functions.Like(t.Category.Description, $"%{description.Value}%")) ||
                         (t.ReceivingAccountId.HasValue && EF.Functions.Like(t.ReceivingAccount.Description, $"%{description.Value}%")));

            var totalCount = query.Count();

            var transactions = query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .Skip(skip)
                .Take(take)
                .ToList();

            return transactions.AsTransactionGroup(totalCount);
        }

        /// <inheritdoc />
        public Transaction UpdateTransaction(int id, int accountId, string description, string dateString, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            this.validator.Description(description);
            var date = this.validator.DateString(dateString, "date");
            var type = this.GetTransactionType(categoryId, receivingAccountId);
            this.validator.Type(type, amount);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Transactions.GetEntity(id);
                if (type != entity.Type) // TODO: Add test for this case
                    throw new ValidationException("Changing the type of transaction is not possible.");

                var account = this.Context.Accounts.GetEntity(accountId, false);

                if (entity.Processed)
                    entity.RevertProcessedTransaction(this.Context);

                var category = categoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                var receivingAccount = receivingAccountId.Select(aId => this.Context.Accounts.GetEntity(aId, false));
                if (receivingAccount.IsSome && receivingAccount.Value.Id == account.Id)
                    throw new ValidationException("Sender account can not be the same as receiver account.");

                entity.AccountId = accountId;
                entity.Account = account;
                entity.Description = description;
                entity.Date = date;
                entity.Amount = amount;
                entity.CategoryId = categoryId.ToNullable();
                entity.Category = category.ToNullIfNone();
                entity.ReceivingAccountId = receivingAccountId.ToNullable();
                entity.ReceivingAccount = receivingAccount.ToNullIfNone();

                // Is confirmed is always filled if needs confirmation is true.
                // ReSharper disable once PossibleInvalidOperationException
                if (date <= LocalDate.FromDateTime(DateTime.Today) && (!entity.NeedsConfirmation || entity.IsConfirmed.Value))
                    entity.ProcessTransaction(this.Context);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public Transaction CreateTransaction(
            int accountId,
            string description,
            string dateString,
            decimal amount,
            Maybe<int> categoryId,
            Maybe<int> receivingAccountId,
            bool needsConfirmation)
        {
            this.validator.Description(description);
            var date = this.validator.DateString(dateString, "date");
            var type = this.GetTransactionType(categoryId, receivingAccountId);
            this.validator.Type(type, amount);

            return this.ConcurrentInvoke(() =>
            {
                var account = this.Context.Accounts.GetEntity(accountId, false);

                var category = categoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                var receivingAccount = receivingAccountId.Select(aId => this.Context.Accounts.GetEntity(aId, false));
                if (receivingAccount.IsSome && receivingAccount.Value.Id == account.Id)
                    throw new ValidationException("Sender account can not be the same as receiver account.");

                var entity = new TransactionEntity
                {
                    Description = description,
                    Type = type,
                    Amount = amount,
                    Date = date,
                    AccountId = accountId,
                    Account = account,
                    Processed = false,
                    CategoryId = categoryId.ToNullable(),
                    Category = category.ToNullIfNone(),
                    ReceivingAccountId = receivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount.ToNullIfNone(),
                    NeedsConfirmation = needsConfirmation,
                    IsConfirmed = needsConfirmation ? false : (bool?)null,
                };

                if (date <= LocalDate.FromDateTime(DateTime.Today) && !needsConfirmation)
                    entity.ProcessTransaction(this.Context);

                this.Context.Transactions.Add(entity);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public Transaction ConfirmTransaction(int id, string dateString, decimal amount)
        {
            var date = this.validator.DateString(dateString, "date");

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Transactions.GetEntity(id);
                this.validator.Type(entity.Type, amount);

                if (!entity.NeedsConfirmation)
                    throw new InvalidOperationException($"This transaction does not need to be confirmed.");

                if (entity.Account.IsObsolete)
                    throw new IsObsoleteException($"Account \"{entity.Account.Description}\" is obsolete.");
                if (entity.CategoryId.HasValue && entity.Category.IsObsolete)
                    throw new IsObsoleteException($"Category \"{entity.Category.Description}\" is obsolete.");
                if (entity.ReceivingAccountId.HasValue && entity.ReceivingAccount.IsObsolete)
                    throw new IsObsoleteException($"Account \"{entity.ReceivingAccount.Description}\" is obsolete.");

                entity.Date = date;
                entity.Amount = amount;
                entity.IsConfirmed = true;

                if (date <= LocalDate.FromDateTime(DateTime.Today))
                    entity.ProcessTransaction(this.Context);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public void DeleteTransaction(int id)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Transactions.GetEntity(id);

                if (entity.Processed)
                    entity.RevertProcessedTransaction(this.Context);

                this.Context.Remove(entity);

                this.Context.SaveChanges();
            });
        }
    }
}