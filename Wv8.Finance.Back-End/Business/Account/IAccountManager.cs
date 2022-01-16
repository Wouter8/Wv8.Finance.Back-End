namespace PersonalFinance.Business.Account
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Interface for the manager providing functionality related to accounts.
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Retrieves an account based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the account.</param>
        /// <returns>The account.</returns>
        Account GetAccount(int id);

        /// <summary>
        /// Retrieves accounts from the database.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete accounts should also be retrieved.</param>
        /// <param name="type">If <c>Some</c>, then only accounts are returned of the provided type.</param>
        /// <returns>The list of accounts.</returns>
        List<Account> GetAccounts(bool includeObsolete, Maybe<AccountType> type);

        /// <summary>
        /// Updates an account.
        /// </summary>
        /// <param name="id">The identifier of the account.</param>
        /// <param name="description">The new description of the account.</param>
        /// <param name="isDefault">Value indicating if the account is the default account.</param>
        /// <param name="iconPack">The new icon pack of the icon for the account.</param>
        /// <param name="iconName">The new name of the icon for the account.</param>
        /// <param name="iconColor">The new background color of the icon for the account.</param>
        /// <returns>The updated account.</returns>
        Account UpdateAccount(int id, string description, bool isDefault, string iconPack, string iconName, string iconColor);

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="type">The type of the account.</param>
        /// <param name="description">The description of the account.</param>
        /// <param name="iconPack">The icon pack of the icon for the account.</param>
        /// <param name="iconName">The name of the icon for the account.</param>
        /// <param name="iconColor">The background color of the icon for the account.</param>
        /// <returns>The created account.</returns>
        Account CreateAccount(AccountType type, string description, string iconPack, string iconName, string iconColor);

        /// <summary>
        /// Sets the obsolete value of an account.
        /// </summary>
        /// <param name="id">The identifier of the account.</param>
        /// <param name="obsolete">The new obsolete value for the account.</param>
        /// <remarks>Nothing happens if the existing obsolete value is the same as the provided obsolete value.</remarks>
        void SetAccountObsolete(int id, bool obsolete);
    }
}