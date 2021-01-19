namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// Service endpoint for actions related to accounts.
    /// </summary>
    [ApiController]
    [Route("api/accounts")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public AccountController(IAccountManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves an account based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the account.</param>
        /// <returns>The account.</returns>
        [HttpGet("{id}")]
        public Account GetAccount(int id)
        {
            return this.manager.GetAccount(id);
        }

        /// <summary>
        /// Retrieves accounts from the database.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete accounts should also be retrieved.</param>
        /// <returns>The list of accounts.</returns>
        [HttpGet]
        public List<Account> GetAccounts(bool includeObsolete)
        {
            return this.manager.GetAccounts(includeObsolete);
        }

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
        [HttpPut("{id}")]
        public Account UpdateAccount(int id, string description, bool isDefault, string iconPack, string iconName, string iconColor)
        {
            return this.manager.UpdateAccount(id, description, isDefault, iconPack, iconName, iconColor);
        }

        /// <summary>
        /// Creates a new account.
        /// </summary>
        /// <param name="type">The type of the account.</param>
        /// <param name="description">The description of the account.</param>
        /// <param name="iconPack">The icon pack of the icon for the account.</param>
        /// <param name="iconName">The name of the icon for the account.</param>
        /// <param name="iconColor">The background color of the icon for the account.</param>
        /// <returns>The created account.</returns>
        [HttpPost]
        public Account CreateAccount(AccountType type, string description, string iconPack, string iconName, string iconColor)
        {
            return this.manager.CreateAccount(type, description, iconPack, iconName, iconColor);
        }

        /// <summary>
        /// Sets the obsolete value of an account.
        /// </summary>
        /// <param name="id">The identifier of the account.</param>
        /// <param name="obsolete">The new obsolete value for the account.</param>
        /// <remarks>Nothing happens if the existing obsolete value is the same as the provided obsolete value.</remarks>
        [HttpPut("obsolete/{id}")]
        public void SetAccountObsolete(int id, bool obsolete)
        {
            this.manager.SetAccountObsolete(id, obsolete);
        }
    }
}