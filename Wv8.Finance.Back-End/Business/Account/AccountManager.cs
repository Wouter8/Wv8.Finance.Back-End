namespace PersonalFinance.Business.Account
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to accounts.
    /// </summary>
    public class AccountManager : BaseManager, IAccountManager
    {
        private readonly AccountValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public AccountManager(Context context)
            : base(context)
        {
            this.validator = new AccountValidator();
        }

        /// <inheritdoc />
        public Account GetAccount(int id)
        {
            return this.Context.Accounts.GetEntity(id).AsAccount();
        }

        /// <inheritdoc />
        public List<Account> GetAccounts(bool includeObsolete, Maybe<AccountType> type)
        {
            return this.Context.Accounts
                .IncludeAll()
                .WhereIf(type.IsSome, a => a.Type == type.Value)
                .WhereIf(!includeObsolete, a => !a.IsObsolete)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Description)
                .Select(a => a.AsAccount())
                .ToList();
        }

        /// <inheritdoc />
        public Account UpdateAccount(int id, string description, bool isDefault, string iconPack, string iconName, string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Accounts.GetEntity(id, false);

                if (this.Context.Accounts.Any(a => a.Id != id && a.Description == description && !a.IsObsolete))
                {
                    throw new ValidationException($"An active account with description \"{description}\" already exists.");
                }

                if (isDefault)
                {
                    if (entity.Type == AccountType.Splitwise)
                        throw new ValidationException($"A Splitwise account can not be the default account.");

                    var defaultAccount = this.Context.Accounts.SingleOrNone(a => a.IsDefault);
                    if (defaultAccount.IsSome)
                    {
                        defaultAccount.Value.IsDefault = false;
                    }
                }

                entity.Description = description;
                entity.IsDefault = isDefault;

                entity.Icon.Name = iconName;
                entity.Icon.Pack = iconPack;
                entity.Icon.Color = iconColor;

                this.Context.SaveChanges();

                return entity.AsAccount();
            });
        }

        /// <inheritdoc />
        public Account CreateAccount(AccountType type, string description, string iconPack, string iconName, string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                if (this.Context.Accounts.Any(a => a.Description == description && !a.IsObsolete))
                    throw new ValidationException($"An active account with description \"{description}\" already exists.");

                if (type == AccountType.Splitwise &&
                    this.Context.Accounts.Any(a => a.Type == AccountType.Splitwise && !a.IsObsolete))
                {
                    throw new ValidationException($"An active Splitwise account already exists.");
                }

                var entity = new DailyBalanceEntity
                {
                    Date = this.Context.CreationTime.ToLocalDate(),
                    Balance = 0,
                    Account = new AccountEntity
                    {
                        Type = type,
                        Description = description,
                        IsDefault = false,
                        IsObsolete = false,
                        Icon = new IconEntity
                        {
                            Pack = iconPack,
                            Name = iconName,
                            Color = iconColor,
                        },
                    },
                };

                this.Context.DailyBalances.Add(entity);
                this.Context.SaveChanges();

                return entity.Account.AsAccount();
            });
        }

        /// <inheritdoc />
        public void SetAccountObsolete(int id, bool obsolete)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Accounts
                    .IncludeAll()
                    .SingleOrNone(a => a.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Account with identifier {id} does not exist."));

                if (obsolete)
                {
                    if (entity.CurrentBalance != 0)
                        throw new ValidationException("This account has a current balance which is not 0.");

                    // Delete any existing recurring transaction for this account
                    // TODO: This should just finish the recurring transaction and not remove it.
                    // TODO: This should remove instances in the future.
                    var recurringTransactions = this.Context.RecurringTransactions
                        .Where(rt => rt.AccountId == entity.Id || rt.ReceivingAccountId == entity.Id)
                        .ToList();
                    var recurringIds = recurringTransactions.Select(rt => rt.Id).ToList();
                    var instances = this.Context.Transactions
                        .Where(t => t.RecurringTransactionId.HasValue && recurringIds.Contains(t.RecurringTransactionId.Value))
                        .ToList();
                    foreach (var instance in instances)
                    {
                        instance.RecurringTransactionId = null;
                    }
                    this.Context.RecurringTransactions.RemoveRange(recurringTransactions);

                    // Obsolete account can not be the default
                    entity.IsDefault = false;
                }
                else
                {
                    // Validate that no other active account exists with the same description.
                    if (this.Context.Accounts.Any(a => a.Description == entity.Description && !a.IsObsolete && a.Id != entity.Id))
                        throw new ValidationException($"An active account with description \"{entity.Description}\" already exists. Change the description of that account first.");

                    if (entity.Type == AccountType.Splitwise && this.Context.Accounts.Any(a => a.Type == AccountType.Splitwise && !a.IsObsolete && a.Id != entity.Id))
                        throw new ValidationException($"An active Splitwise account already exists.");
                }

                entity.IsObsolete = obsolete;

                this.Context.SaveChanges();
            });
        }
    }
}