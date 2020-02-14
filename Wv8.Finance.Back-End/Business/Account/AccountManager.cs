namespace PersonalFinance.Business.Account
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using PersonalFinance.Common.DataTransfer;
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
        public List<Account> GetAccounts(bool includeObsolete)
        {
            return this.Context.Accounts
                .IncludeAll()
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
                var entity = this.Context.Accounts.GetEntity(id);

                if (this.Context.Accounts.Any(a => a.Id != id && a.Description == description && !a.IsObsolete))
                {
                    throw new ValidationException($"An active account with description \"{description}\" already exists.");
                }

                if (isDefault)
                {
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
        public Account CreateAccount(string description, string iconPack, string iconName, string iconColor)
        {
            description = this.validator.Description(description);
            this.validator.Icon(iconPack, iconName, iconColor);

            return this.ConcurrentInvoke(() =>
            {
                if (this.Context.Accounts.Any(a => a.Description == description && !a.IsObsolete))
                    throw new ValidationException($"An active account with description \"{description}\" already exists.");

                var entity = new AccountEntity
                {
                    Description = description,
                    CurrentBalance = 0,
                    IsDefault = false,
                    IsObsolete = false,
                    Icon = new IconEntity
                    {
                        Pack = iconPack,
                        Name = iconName,
                        Color = iconColor,
                    },
                };

                this.Context.Accounts.Add(entity);
                this.Context.SaveChanges();

                return entity.AsAccount();
            });
        }

        /// <inheritdoc />
        public void SetAccountObsolete(int id, bool obsolete)
        {
            this.ConcurrentInvoke(() =>
            {
                var entity = this.Context.Accounts
                    .SingleOrNone(a => a.Id == id)
                    .ValueOrThrow(() => new DoesNotExistException($"Account with identifier {id} does not exist."));

                if (obsolete)
                {
                    entity.IsDefault = false;
                }
                else
                {
                    // Validate that no other active account exists with the same description.
                    if (this.Context.Accounts.Any(a => a.Description == entity.Description && !a.IsObsolete && a.Id != entity.Id))
                        throw new ValidationException($"An active account with description \"{entity.Description}\" already exists. Change the description of that account first.");
                }

                entity.IsObsolete = obsolete;

                this.Context.SaveChanges();
            });
        }
    }
}