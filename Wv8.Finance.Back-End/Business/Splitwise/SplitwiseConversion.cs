namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using SW = PersonalFinance.Data.External.Splitwise.Models;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class SplitwiseConversion
    {
        /// <summary>
        /// Updates the values of a Splitwise transaction with the values from an expense.
        /// </summary>
        /// <param name="entity">The Splitwise transaction entity to be updated.</param>
        /// <param name="expense">The expense.</param>
        public static void UpdateValues(this SplitwiseTransactionEntity entity, SW.Expense expense)
        {
            entity.Id = expense.Id;
            entity.Date = expense.Date;
            entity.Description = expense.Description;
            entity.IsDeleted = expense.IsDeleted;
            entity.UpdatedAt = expense.UpdatedAt;
            entity.PaidAmount = expense.PaidAmount;
            entity.PersonalAmount = expense.PersonalAmount;
            entity.Imported = false;
        }

        /// <summary>
        /// Converts an expense from Splitwise to a Splitwise transaction entity.
        /// </summary>
        /// <param name="expense">The expense.</param>
        /// <returns>The created entity.</returns>
        public static SplitwiseTransactionEntity ToSplitwiseTransactionEntity(this SW.Expense expense)
        {
            return new SplitwiseTransactionEntity
            {
                Id = expense.Id,
                Date = expense.Date,
                Description = expense.Description,
                Imported = false,
                IsDeleted = expense.IsDeleted,
                PaidAmount = expense.PaidAmount,
                PersonalAmount = expense.PersonalAmount,
                UpdatedAt = expense.UpdatedAt,
                SplitDetails = expense.Splits.Select(s => s.ToSplitDetailEntity()).ToList(),
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitwiseTransaction AsSplitwiseTransaction(this SplitwiseTransactionEntity entity)
        {
            return new SplitwiseTransaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Date = entity.Date,
                IsDeleted = entity.IsDeleted,
                Imported = entity.Imported,
                PaidAmount = entity.PaidAmount,
                PersonalAmount = entity.PersonalAmount,
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitDetailEntity ToSplitDetailEntity(this SW.Split split)
        {
            return new SplitDetailEntity
            {
                SplitwiseUserId = split.UserId,
                SplitwiseUserName = split.UserName,
                Amount = split.Amount,
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitDetail AsSplitDetail(this SplitDetailEntity entity)
        {
            return new SplitDetail
            {
                TransactionId = entity.TransactionId.ToMaybe(),
                SplitwiseTransactionId = entity.SplitwiseTransactionId.ToMaybe(),
                SplitwiseUserId = entity.SplitwiseUserId,
                SplitwiseUserName = entity.SplitwiseUserName,
                Amount = entity.Amount,
            };
        }

        /// <summary>
        /// Converts the user to a data transfer object.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitwiseUser AsSplitwiseUser(this SW.User user)
        {
            return new SplitwiseUser
            {
                Id = user.Id,
                Name = user.Name,
            };
        }

        /// <summary>
        /// Converts a Splitwise transaction to a normal transaction. Creates a transaction with the correct type.
        /// Also marks the Splitwise transaction as imported.
        /// </summary>
        /// <param name="entity">The Splitwise transaction.</param>
        /// <param name="account">The account to be linked to the transaction.</param>
        /// <param name="category">The category to be linked to the transaction.</param>
        /// <returns>The created transaction.</returns>
        public static TransactionEntity ToTransaction(
            this SplitwiseTransactionEntity entity, AccountEntity account, CategoryEntity category)
        {
            entity.Imported = true;

            var transaction = new TransactionEntity
            {
                Description = entity.Description,
                Date = entity.Date,
                AccountId = account.Id,
                Account = account,
                CategoryId = category.Id,
                Category = category,
                SplitwiseTransactionId = entity.Id,
                SplitwiseTransaction = entity,
                PaymentRequests = new List<PaymentRequestEntity>(),
                SplitDetails = entity.SplitDetails,
            };

            if (entity.PaidAmount > 0 && account.Type == AccountType.Splitwise)
            {
                transaction.Type = TransactionType.Income;
                transaction.Amount = entity.PaidAmount - entity.PersonalAmount;
            }
            else
            {
                transaction.Type = TransactionType.Expense;
                transaction.Amount = -entity.PaidAmount;
            }

            return transaction;
        }

        /// <summary>
        /// Converts a Splitwise transaction to a transfer transaction. Creates a transaction with the correct type.
        /// Also marks the Splitwise transaction as imported.
        /// </summary>
        /// <param name="entity">The Splitwise transaction.</param>
        /// <param name="splitwiseAccount">The Spltiwise account.</param>
        /// <param name="internalAccount">The receiving/sending account.</param>
        /// <returns>The created transaction.</returns>
        public static TransactionEntity ToTransaction(
            this SplitwiseTransactionEntity entity, AccountEntity splitwiseAccount, AccountEntity internalAccount)
        {
            entity.Imported = true;

            var transaction = new TransactionEntity
            {
                Description = entity.Description,
                Date = entity.Date,
                SplitwiseTransactionId = entity.Id,
                SplitwiseTransaction = entity,
                PaymentRequests = new List<PaymentRequestEntity>(),
                SplitDetails = entity.SplitDetails,
                Type = TransactionType.Transfer,
            };

            if (entity.PaidAmount > 0)
            {
                transaction.Account = internalAccount;
                transaction.AccountId = internalAccount.Id;
                transaction.ReceivingAccount = splitwiseAccount;
                transaction.ReceivingAccountId = splitwiseAccount.Id;
                transaction.Amount = entity.PaidAmount;
            }
            else
            {
                transaction.Account = splitwiseAccount;
                transaction.AccountId = splitwiseAccount.Id;
                transaction.ReceivingAccount = internalAccount;
                transaction.ReceivingAccountId = internalAccount.Id;
                transaction.Amount = entity.PersonalAmount;
            }

            return transaction;
        }
    }
}
