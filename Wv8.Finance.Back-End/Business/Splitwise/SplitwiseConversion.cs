namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class SplitwiseConversion
    {
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
                PaidAmount = entity.PaidAmount,
                PersonalAmount = entity.PersonalAmount,
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
                Account = account,
                Category = category,
                SplitwiseTransaction = entity,
                PaymentRequests = new List<PaymentRequestEntity>(),
            };

            // If the transaction is (partly) mine, then create an expense transaction.
            if (entity.PersonalAmount > 0)
            {
                transaction.Type = TransactionType.Expense;
                // If I paid anything, then use the paid amount since balances have to be updated properly.
                // If not, then just use the amount owed to others to get the correct balance.
                transaction.Amount = entity.PaidAmount > 0 ? entity.PaidAmount : entity.OwedToOthers;
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