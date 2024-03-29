﻿namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.External.Splitwise;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
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
        /// The Splitwise context.
        /// </summary>
        private readonly ISplitwiseContext splitwiseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTransactionManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public RecurringTransactionManager(Context context, ISplitwiseContext splitwiseContext)
            : base(context)
        {
            this.validator = new TransactionValidator();
            this.splitwiseContext = splitwiseContext;
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
            int id, InputRecurringTransaction input, bool updateInstances)
        {
            this.validator.Description(input.Description);
            var startPeriod = this.validator.DateString(input.StartDateString, "startDate");
            var endPeriod = input.EndDateString.Select(d => this.validator.DateString(d, "endDate"));
            if (endPeriod.IsSome)
                this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(input.Interval);
            var type = this.GetTransactionType(input.CategoryId, input.ReceivingAccountId, input.Amount);
            this.validator.Splits(this.splitwiseContext, input.PaymentRequests, input.SplitwiseSplits, type, input.Amount);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                var entity = this.Context.RecurringTransactions.GetEntity(id);

                this.validator.AccountType(entity.Account.Type);
                if (entity.ReceivingAccount != null)
                    this.validator.AccountType(entity.ReceivingAccount.Type);

                if (type != entity.Type) // TODO: Add test for this case
                    throw new ValidationException("Changing the type of transaction is not possible.");

                if (!updateInstances && startPeriod != entity.StartDate)
                    throw new ValidationException($"Updating the start date without updating already created instances is not supported.");

                var account = this.Context.Accounts.GetEntity(input.AccountId, false);

                this.validator.AccountType(account.Type);

                var category = input.CategoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                AccountEntity receivingAccount = null;
                if (input.ReceivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(input.ReceivingAccountId.Value, false);

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");

                    this.validator.AccountType(receivingAccount.Type);
                }

                var splits = new List<SplitDetailEntity>();
                if (input.SplitwiseSplits.Any())
                {
                    // Verify a Splitwise account exists when adding providing splits.
                    this.Context.Accounts.GetSplitwiseEntity();
                    var splitwiseUsers = this.splitwiseContext.GetUsers().ToDictionary(su => su.Id);
                    if (input.SplitwiseSplits.Any(s => !splitwiseUsers.ContainsKey(s.UserId)))
                        throw new ValidationException("Unknown Splitwise user specified.");
                    splits = input.SplitwiseSplits.Select(s => s.ToSplitDetailEntity(splitwiseUsers)).ToList();
                }

                entity.AccountId = input.AccountId;
                entity.Account = account;
                entity.Description = input.Description;
                entity.StartDate = startPeriod;
                entity.EndDate = endPeriod.ToNullable();
                entity.Amount = input.Amount;
                entity.CategoryId = input.CategoryId.ToNullable();
                entity.Category = category.ToNullIfNone();
                entity.ReceivingAccountId = input.ReceivingAccountId.ToNullable();
                entity.ReceivingAccount = receivingAccount;
                entity.NeedsConfirmation = input.NeedsConfirmation;
                entity.Interval = input.Interval;
                entity.IntervalUnit = input.IntervalUnit;
                entity.SplitDetails = splits;

                var instances = this.Context.Transactions.GetTransactionsFromRecurring(entity.Id);
                var instancesToUpdate = updateInstances
                    ? instances.ToList() // Copy the list
                    // Always update all unprocessed transactions.
                    : instances.Where(t => !t.Processed).ToList();

                foreach (var instance in instancesToUpdate)
                {
                    processor.RevertIfProcessed(instance);
                    instances.Remove(instance);
                    this.Context.Remove(instance);
                }

                entity.LastOccurence = instances
                    .OrderByDescending(t => t.Date)
                    .FirstOrNone()
                    .Select(t => (LocalDate?)t.Date)
                    .ValueOrElse(() => (LocalDate?)null);
                entity.SetNextOccurrence();

                processor.Process(entity);

                this.Context.SaveChanges();

                return entity.AsRecurringTransaction();
            });
        }

        /// <inheritdoc />
        public RecurringTransaction CreateRecurringTransaction(InputRecurringTransaction input)
        {
            this.validator.Description(input.Description);
            var startPeriod = this.validator.DateString(input.StartDateString, "startDate");
            var endPeriod = input.EndDateString.Select(d => this.validator.DateString(d, "endDate"));
            if (endPeriod.IsSome)
                this.validator.Period(startPeriod, endPeriod);
            this.validator.Interval(input.Interval);
            var type = this.GetTransactionType(input.CategoryId, input.ReceivingAccountId, input.Amount);
            this.validator.Splits(this.splitwiseContext, input.PaymentRequests, input.SplitwiseSplits, type, input.Amount);

            return this.ConcurrentInvoke(() =>
            {
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                var account = this.Context.Accounts.GetEntity(input.AccountId, false);

                this.validator.AccountType(account.Type);

                var category = input.CategoryId.Select(cId => this.Context.Categories.GetEntity(cId, false));

                AccountEntity receivingAccount = null;
                if (input.ReceivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(input.ReceivingAccountId.Value, false);

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");

                    this.validator.AccountType(receivingAccount.Type);
                }

                var splits = new List<SplitDetailEntity>();
                if (input.SplitwiseSplits.Any())
                {
                    // Verify a Splitwise account exists when adding providing splits.
                    this.Context.Accounts.GetSplitwiseEntity();
                    var splitwiseUsers = this.splitwiseContext.GetUsers().ToDictionary(su => su.Id);
                    if (input.SplitwiseSplits.Any(s => !splitwiseUsers.ContainsKey(s.UserId)))
                        throw new ValidationException("Unknown Splitwise user specified.");
                    splits = input.SplitwiseSplits.Select(s => s.ToSplitDetailEntity(splitwiseUsers)).ToList();
                }

                var entity = new RecurringTransactionEntity
                {
                    Description = input.Description,
                    Type = type,
                    Amount = input.Amount,
                    StartDate = startPeriod,
                    EndDate = endPeriod.ToNullable(),
                    AccountId = input.AccountId,
                    Account = account,
                    CategoryId = input.CategoryId.ToNullable(),
                    Category = category.ToNullIfNone(),
                    ReceivingAccountId = input.ReceivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount,
                    Interval = input.Interval,
                    IntervalUnit = input.IntervalUnit,
                    NeedsConfirmation = input.NeedsConfirmation,
                    NextOccurence = startPeriod,
                    PaymentRequests = new List<PaymentRequestEntity>(), // TODO: Payment requests
                    SplitDetails = splits,
                };

                processor.Process(entity);

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
                var processor = new TransactionProcessor(this.Context, this.splitwiseContext);

                var entity = this.Context.RecurringTransactions.GetEntity(id);

                var instances = this.Context.Transactions.GetTransactionsFromRecurring(entity.Id);
                foreach (var instance in instances)
                {
                    if (deleteInstances)
                    {
                        processor.RevertIfProcessed(instance);
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
