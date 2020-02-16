// ReSharper disable PossibleInvalidOperationException
namespace PersonalFinance.Business.Transaction
{
    using System;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class providing extension methods to settle transactions.
    /// </summary>
    public static class SettleExtensions
    {
        /// <summary>
        /// Settles a transaction. Meaning the value is added to the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="context">The database context.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        /// <returns>The updated transaction.</returns>
        public static TransactionEntity ProcessTransaction(this TransactionEntity transaction, Context context)
        {
            // Update account balance.
            var account = context.Accounts.GetEntity(transaction.AccountId);

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    account.CurrentBalance -= Math.Abs(transaction.Amount);

                    // Update budgets.
                    var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                    {
                        budget.Spent += Math.Abs(transaction.Amount);
                    }
                    break;
                case TransactionType.Income:
                    account.CurrentBalance += transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    var receiver = context.Accounts.GetEntity(transaction.ReceivingAccountId.Value);
                    account.CurrentBalance -= transaction.Amount;
                    receiver.CurrentBalance += transaction.Amount;

                    // TODO: Savings
                    break;
            }

            transaction.Processed = true;

            return transaction;
        }

        /// <summary>
        /// Reverses a settlement of a transaction. Meaning the value is removed from the account, budgets and savings.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="context">The database context.</param>
        /// <remarks>Note that the context is not saved.</remarks>
        /// <returns>The updated transaction.</returns>
        public static TransactionEntity UnprocessTransaction(this TransactionEntity transaction, Context context)
        {
            // Update account balance.
            var account = context.Accounts.GetEntity(transaction.AccountId);

            switch (transaction.Type)
            {
                case TransactionType.Expense:
                    account.CurrentBalance += Math.Abs(transaction.Amount);

                    // Update budgets.
                    var budgets = context.Budgets.GetBudgets(transaction.CategoryId.Value, transaction.Date);
                    foreach (var budget in budgets)
                    {
                        budget.Spent -= Math.Abs(transaction.Amount);
                    }
                    break;
                case TransactionType.Income:
                    account.CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Transfer:
                    var receiver = context.Accounts.GetEntity(transaction.ReceivingAccountId.Value);
                    account.CurrentBalance += transaction.Amount;
                    receiver.CurrentBalance -= transaction.Amount;

                    // TODO: Savings
                    break;
            }

            transaction.Processed = false;

            return transaction;
        }
    }
}