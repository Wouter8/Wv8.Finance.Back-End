namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
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
        public Transaction UpdateTransaction(EditTransaction input)
        {
            this.validator.Description(input.Description);
            var date = this.validator.DateString(input.DateString, "date");
            var type = this.GetTransactionType(input.CategoryId, input.ReceivingAccountId, input.Amount);
            this.validator.PaymentRequests(input.PaymentRequests, type, input.Amount);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context);

                var entity = this.Context.Transactions.GetEntity(input.Id);
                if (type != entity.Type)
                    throw new ValidationException("Changing the type of transaction is not possible.");

                var account = this.Context.Accounts.GetEntity(input.AccountId, false);

                processor.RevertIfProcessed(entity);

                var category = input.CategoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                var receivingAccount = input.ReceivingAccountId.Select(aId => this.Context.Accounts.GetEntity(aId, false));
                if (receivingAccount.IsSome && receivingAccount.Value.Id == account.Id)
                    throw new ValidationException("Sender account can not be the same as receiver account.");

                entity.AccountId = input.AccountId;
                entity.Account = account;
                entity.Description = input.Description;
                entity.Date = date;
                entity.Amount = input.Amount;
                entity.CategoryId = input.CategoryId.ToNullable();
                entity.Category = category.ToNullIfNone();
                entity.ReceivingAccountId = input.ReceivingAccountId.ToNullable();
                entity.ReceivingAccount = receivingAccount.ToNullIfNone();

                var existingPaymentRequestIds = input.PaymentRequests
                    .SelectSome(pr => pr.Id)
                    .ToSet();
                var existingPaymentRequests = entity.PaymentRequests
                    .Where(pr => existingPaymentRequestIds.Contains(pr.Id))
                    .ToDictionary(pr => pr.Id);

                var updatedPaymentRequests = new List<PaymentRequestEntity>();

                foreach (var inputPr in input.PaymentRequests)
                {
                    var updatedPr = inputPr.Id
                        .Select(id => existingPaymentRequests[id])
                        .ValueOrElse(new PaymentRequestEntity());

                    if (updatedPr.PaidCount > inputPr.Count)
                        throw new ValidationException("A payment request can not be updated resulting in more payments than requested.");

                    updatedPr.Amount = inputPr.Amount;
                    updatedPr.Count = inputPr.Count;
                    updatedPr.Name = inputPr.Name;

                    updatedPaymentRequests.Add(updatedPr);
                }

                var updatedPaymentRequestIds = updatedPaymentRequests.Select(pr => pr.Id).ToSet();
                var removedPaymentRequests =
                    entity.PaymentRequests.Where(pr => !updatedPaymentRequestIds.Contains(pr.Id));

                this.Context.PaymentRequests.RemoveRange(removedPaymentRequests);
                entity.PaymentRequests = updatedPaymentRequests;

                processor.ProcessIfNeeded(entity);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public Transaction CreateTransaction(InputTransaction input)
        {
            this.validator.Description(input.Description);
            var date = this.validator.DateString(input.DateString, "date");
            var type = this.GetTransactionType(input.CategoryId, input.ReceivingAccountId, input.Amount);
            this.validator.PaymentRequests(input.PaymentRequests, type, input.Amount);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context);

                var account = this.Context.Accounts.GetEntity(input.AccountId, false);

                var category = input.CategoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                var receivingAccount = input.ReceivingAccountId.Select(aId => this.Context.Accounts.GetEntity(aId, false));
                if (receivingAccount.IsSome && receivingAccount.Value.Id == account.Id)
                    throw new ValidationException("Sender account can not be the same as receiver account.");

                var entity = new TransactionEntity
                {
                    Description = input.Description,
                    Type = type,
                    Amount = input.Amount,
                    Date = date,
                    AccountId = input.AccountId,
                    Account = account,
                    Processed = false,
                    CategoryId = input.CategoryId.ToNullable(),
                    Category = category.ToNullIfNone(),
                    ReceivingAccountId = input.ReceivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount.ToNullIfNone(),
                    NeedsConfirmation = input.NeedsConfirmation,
                    IsConfirmed = input.NeedsConfirmation ? false : (bool?)null,
                    PaymentRequests = input.PaymentRequests.Select(pr => new PaymentRequestEntity
                    {
                        Amount = pr.Amount,
                        Name = pr.Name,
                        Count = pr.Count,
                    }).ToList(),
                };

                processor.ProcessIfNeeded(entity);

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
                var processor = new TransactionProcessor(this.Context);

                var entity = this.Context.Transactions.GetEntity(id);
                this.validator.Amount(amount, entity.Type);

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

                processor.ProcessIfNeeded(entity);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public void DeleteTransaction(int id)
        {
            this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context);

                var entity = this.Context.Transactions.GetEntity(id);

                processor.RevertIfProcessed(entity);

                this.Context.Remove(entity);

                this.Context.SaveChanges();
            });
        }

        /// <inheritdoc />
        public PaymentRequest FulfillPaymentRequest(int id)
        {
            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.PaymentRequests.GetEntity(id);

                if (entity.Completed)
                    throw new ValidationException("This payment request is already completed.");

                entity.PaidCount++;

                this.Context.SaveChanges();

                return entity.AsPaymentRequest();
            });
        }

        /// <inheritdoc />
        public PaymentRequest RevertPaymentPaymentRequest(int id)
        {
            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.PaymentRequests.GetEntity(id);

                if (entity.PaidCount == 0)
                    throw new ValidationException("This payment request has not yet been paid.");

                entity.PaidCount--;

                this.Context.SaveChanges();

                return entity.AsPaymentRequest();
            });
        }
    }
}