namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Linq;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
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
            var startPeriod = startDate.Select(d => this.validator.IsoString(d, nameof(startDate)));
            var endPeriod = endDate.Select(d => this.validator.IsoString(d, nameof(endDate)));
            this.validator.Pagination(skip, take);
            this.validator.Period(startPeriod, endPeriod, true);

            var allTransactions = this.Context.Transactions
                .IncludeAll()
                .WhereIf(type.IsSome, t => t.Type == type.Value)
                .WhereIf(accountId.IsSome, t => t.AccountId == accountId.Value || t.ReceivingAccountId == accountId.Value)
                .WhereIf(
                    categoryId.IsSome,
                    t => t.CategoryId.HasValue && (t.CategoryId.Value == categoryId.Value ||
                                                   (t.Category.ParentCategoryId.HasValue &&
                                                    t.Category.ParentCategoryId.Value == categoryId.Value)))
                .WhereIf(startPeriod.IsSome, t => startPeriod.Value <= t.Date && endPeriod.Value >= t.Date)
                .OrderByDescending(t => t.Date)
                .ToList()
                .WhereIf( // Not translatable
                    description.IsSome,
                    t => t.Description.Contains(description.Value, StringComparison.InvariantCultureIgnoreCase) ||
                         t.Account.Description.Contains(description.Value, StringComparison.InvariantCultureIgnoreCase) ||
                         (t.CategoryId.HasValue && t.Category.Description.Contains(description.Value, StringComparison.InvariantCultureIgnoreCase)) ||
                         (t.ReceivingAccountId.HasValue && t.ReceivingAccount.Description.Contains(description.Value, StringComparison.InvariantCultureIgnoreCase)))
                .ToList();

            return allTransactions.Skip(skip)
                .Take(take)
                .ToList()
                .AsTransactionGroup(allTransactions.Count);
        }

        /// <inheritdoc />
        public Transaction UpdateTransaction(int id, int accountId, string description, string isoDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            this.validator.Description(description);
            var date = this.validator.IsoString(isoDate, "date");

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Transactions.GetEntity(id);
                this.validator.Type(entity.Type, amount, categoryId, receivingAccountId);

                var account = this.Context.Accounts.GetEntity(accountId);
                if (account.IsObsolete)
                    throw new ValidationException("Account is obsolete.");

                if (entity.Processed)
                    entity.RevertProcessedTransaction(this.Context);

                CategoryEntity category = null;
                if (categoryId.IsSome)
                {
                    category = this.Context.Categories.GetEntity(categoryId.Value);
                    if (category.IsObsolete)
                        throw new ValidationException("Category is obsolete.");
                    if (entity.Type == TransactionType.Expense && category.Type != CategoryType.Expense)
                        throw new ValidationException($"Category \"{category.Description}\" is not an expense category.");
                    if (entity.Type == TransactionType.Income && category.Type != CategoryType.Income)
                        throw new ValidationException($"Category \"{category.Description}\" is not an income category.");
                }

                AccountEntity receivingAccount = null;
                if (receivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(receivingAccountId.Value);
                    if (receivingAccount.IsObsolete)
                        throw new ValidationException("Receiving account is obsolete.");

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");
                }

                entity.AccountId = accountId;
                entity.Account = account;
                entity.Description = description;
                entity.Date = date;
                entity.Amount = amount;
                entity.CategoryId = categoryId.ToNullable();
                entity.Category = category;
                entity.ReceivingAccountId = receivingAccountId.ToNullable();
                entity.ReceivingAccount = receivingAccount;

                if (date <= DateTime.Today)
                    entity.ProcessTransaction(this.Context);

                this.Context.SaveChanges();

                return entity.AsTransaction();
            });
        }

        /// <inheritdoc />
        public Transaction CreateTransaction(int accountId, TransactionType type, string description, string isoDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            this.validator.Description(description);
            var date = this.validator.IsoString(isoDate, "date");
            this.validator.Type(type, amount, categoryId, receivingAccountId);

            return this.ConcurrentInvoke(() =>
            {
                var account = this.Context.Accounts.GetEntity(accountId);
                if (account.IsObsolete)
                    throw new ValidationException("Account is obsolete.");

                CategoryEntity category = null;
                if (categoryId.IsSome)
                {
                    category = this.Context.Categories.GetEntity(categoryId.Value);
                    if (category.IsObsolete)
                        throw new ValidationException("Category is obsolete.");
                    if (type == TransactionType.Expense && category.Type != CategoryType.Expense)
                        throw new ValidationException($"Category \"{category.Description}\" is not an expense category.");
                    if (type == TransactionType.Income && category.Type != CategoryType.Income)
                        throw new ValidationException($"Category \"{category.Description}\" is not an income category.");
                }

                AccountEntity receivingAccount = null;
                if (receivingAccountId.IsSome)
                {
                    receivingAccount = this.Context.Accounts.GetEntity(receivingAccountId.Value);
                    if (receivingAccount.IsObsolete)
                        throw new ValidationException("Receiving account is obsolete.");

                    if (receivingAccount.Id == account.Id)
                        throw new ValidationException("Sender account can not be the same as receiver account.");
                }

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
                    Category = category,
                    ReceivingAccountId = receivingAccountId.ToNullable(),
                    ReceivingAccount = receivingAccount,
                };

                if (date <= DateTime.Today)
                    entity.ProcessTransaction(this.Context);

                this.Context.Transactions.Add(entity);

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