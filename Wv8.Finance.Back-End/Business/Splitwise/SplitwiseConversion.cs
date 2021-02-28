namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;
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
        /// <returns>The updated entity.</returns>
        public static SplitwiseTransactionEntity UpdateValues(this SplitwiseTransactionEntity entity, SW.Expense expense)
        {
            entity.Id = expense.Id;
            entity.Date = expense.Date;
            entity.Description = expense.Description;
            entity.IsDeleted = expense.IsDeleted;
            entity.UpdatedAt = expense.UpdatedAt;
            entity.PaidAmount = expense.PaidAmount;
            entity.PersonalAmount = expense.PersonalAmount;

            return entity;
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
        /// Converts the user to a data transfer object.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitwiseUser AsSplitwiseUser(this SW.User user)
        {
            var name = user.LastName.IsSome
                ? $"{user.FirstName} {user.LastName.Value}"
                : $"{user.FirstName}";
            return new SplitwiseUser
            {
                Id = user.Id,
                Name = name,
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
            };

            // If the transaction is (partly) mine, then create an expense transaction.
            if (entity.PersonalAmount > 0)
            {
                transaction.Type = TransactionType.Expense;
                // The amount is equal to what is actually paid. The personal amount will be calculated.
                transaction.Amount = -entity.PaidAmount;
            }
            // Otherwise create an income transaction.
            else
            {
                transaction.Type = TransactionType.Income;
                transaction.Amount = entity.OwedByOthers;
            }

            return transaction;
        }
    }
}