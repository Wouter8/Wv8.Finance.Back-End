namespace Business.UnitTest.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// Tests for the account manager.
    /// </summary>
    public class AccountTests : BaseTest
    {
        #region GetAccount

        /// <summary>
        /// Tests the good flow of the GetAccount method.
        /// </summary>
        [Fact]
        public void GetAccount()
        {
            var savedAccount = this.GenerateAccount();
            var transaction = this.GenerateTransaction(accountId: savedAccount.Id, amount: -50);

            var retrievedAccount = this.AccountManager.GetAccount(savedAccount.Id);

            Assert.Equal(savedAccount.Id, retrievedAccount.Id);
            Assert.Equal(savedAccount.Description, retrievedAccount.Description);
            Assert.Equal(-50, retrievedAccount.CurrentBalance);
            Assert.Equal(savedAccount.IsDefault, retrievedAccount.IsDefault);
            Assert.Equal(savedAccount.IsObsolete, retrievedAccount.IsObsolete);
            Assert.Equal(savedAccount.IconId, retrievedAccount.IconId);
        }

        /// <summary>
        /// Tests the exceptional flow of the GetAccount method.
        /// </summary>
        [Fact]
        public void GetAccount_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.AccountManager.GetAccount(-1));
        }

        #endregion GetAccount

        #region GetAccounts

        /// <summary>
        /// Tests the good flow of the GetAccounts method.
        /// </summary>
        [Fact]
        public void GetAccounts()
        {
            // Empty database.
            var retrievedAccounts = this.AccountManager.GetAccounts(true);
            Assert.Empty(retrievedAccounts);

            // Create accounts.
            const int accountCount = 5;
            var savedAccounts = new List<Account>();
            for (int i = 0; i < accountCount; i++)
            {
                savedAccounts.Add(this.GenerateAccount());
            }

            // Set second account as default.
            const int defaultAccountIndex = 1;
            this.AccountManager.UpdateAccount(
                savedAccounts[defaultAccountIndex].Id,
                savedAccounts[defaultAccountIndex].Description,
                true,
                savedAccounts[defaultAccountIndex].Icon.Pack,
                savedAccounts[defaultAccountIndex].Icon.Name,
                savedAccounts[defaultAccountIndex].Icon.Color);
            savedAccounts[defaultAccountIndex] = this.AccountManager.GetAccount(savedAccounts[defaultAccountIndex].Id);

            // Load all active accounts (all active). Verify default account is first.
            retrievedAccounts = this.AccountManager.GetAccounts(false);
            Assert.Equal(savedAccounts[defaultAccountIndex].Id, retrievedAccounts.First().Id);
            Assert.Equal(accountCount, retrievedAccounts.Count);

            // Verify accounts.
            foreach (var savedAccount in savedAccounts)
            {
                var retrievedAccount = retrievedAccounts.Single(a => a.Id == savedAccount.Id);

                this.AssertEqual(savedAccount, retrievedAccount);
            }

            // Load active and inactive accounts (all active).
            retrievedAccounts = this.AccountManager.GetAccounts(true);
            Assert.Equal(accountCount, retrievedAccounts.Count);

            // Set account obsolete
            this.AccountManager.SetAccountObsolete(savedAccounts.Last().Id, true);

            // Load all active accounts (all except 1)
            retrievedAccounts = this.AccountManager.GetAccounts(false);
            Assert.Equal(accountCount - 1, retrievedAccounts.Count);

            // Load active and inactive accounts (should return all again).
            retrievedAccounts = this.AccountManager.GetAccounts(true);
            Assert.Equal(accountCount, retrievedAccounts.Count);
        }

        #endregion GetAccounts

        #region UpdateAccount

        /// <summary>
        /// Tests the good flow of the UpdateAccount method.
        /// </summary>
        [Fact]
        public void UpdateAccount()
        {
            var account = this.GenerateAccount();

            const string newDescription = "Description";
            const bool newIsDefault = true;
            const string newIconPack = "fas";
            const string newIconName = "circle";
            const string newIconColor = "#FFFFFF";

            var updated = this.AccountManager.UpdateAccount(account.Id, newDescription, newIsDefault, newIconPack, newIconName, newIconColor);

            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newIsDefault, updated.IsDefault);
            Assert.Equal(updated.Icon.Pack, newIconPack);
            Assert.Equal(updated.Icon.Name, newIconName);
            Assert.Equal(updated.Icon.Color, newIconColor);

            // Add new account and mark as default.
            var account2 = this.GenerateAccount();
            this.AccountManager.UpdateAccount(account2.Id, account2.Description, true, account2.Icon.Pack, account2.Icon.Name, account2.Icon.Color);

            // Retrieve old default account and verify it is no longer default.
            updated = this.AccountManager.GetAccount(account.Id);
            Assert.False(updated.IsDefault);
        }

        /// <summary>
        /// Tests the exceptional flow of the UpdateAccount method.
        /// </summary>
        [Fact]
        public void UpdateAccount_Exceptions()
        {
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount("Description");

            const string newDescription = "Description";
            const bool isDefault = true;
            const string newIconPack = "fas";
            const string newIconName = "circle";
            const string newIconColor = "#FFFFFF";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.AccountManager.UpdateAccount(account.Id, newDescription, isDefault, newIconPack, newIconName, newIconColor));

            // Account with same description is obsolete.
            this.AccountManager.SetAccountObsolete(account2.Id, true);
            this.AccountManager.UpdateAccount(account.Id, newDescription, isDefault, newIconPack, newIconName, newIconColor);

            Assert.Throws<DoesNotExistException>(
                () => this.AccountManager.UpdateAccount(100, newDescription, isDefault, newIconPack, newIconName, newIconColor));
        }

        #endregion UpdateAccount

        #region CreateAccount

        /// <summary>
        /// Tests the good flow of the CreateAccount method.
        /// </summary>
        [Fact]
        public void CreateAccount()
        {
            const string description = "Description";
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            var account = this.AccountManager.CreateAccount(description, iconPack, iconName, iconColor);

            Assert.Equal(description, account.Description);
            Assert.Equal(iconPack, account.Icon.Pack);
            Assert.Equal(iconName, account.Icon.Name);
            Assert.Equal(iconColor, account.Icon.Color);
            Assert.False(account.IsDefault);
            Assert.False(account.IsObsolete);
        }

        /// <summary>
        /// Tests the exceptional flow of the CreateAccount method.
        /// </summary>
        [Fact]
        public void CreateAccount_Exceptions()
        {
            var account = this.GenerateAccount("Description");

            const string description = "Description";
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.AccountManager.CreateAccount(description, iconPack, iconName, iconColor));
        }

        #endregion CreateAccount

        #region SetAccountObsolete

        /// <summary>
        /// Tests the good flow of the SetAccountObsolete method.
        /// </summary>
        [Fact]
        public void SetAccountObsolete()
        {
            var account = this.GenerateAccount();
            var account2 = this.GenerateAccount();

            // Create 2 recurring transactions for account.
            var rTransaction1 = this.GenerateRecurringTransaction(account.Id);
            var rTransaction2 = this.GenerateRecurringTransaction(
                accountId: account2.Id, type: TransactionType.Internal, receivingAccountId: account.Id);

            // Set as default and obsolete.
            this.AccountManager.UpdateAccount(
                account.Id,
                account.Description,
                true,
                account.Icon.Pack,
                account.Icon.Name,
                account.Icon.Color);
            this.AccountManager.SetAccountObsolete(account.Id, true);

            var updated = this.AccountManager.GetAccount(account.Id);

            // Verify account is no longer default and is obsolete.
            Assert.True(updated.IsObsolete);
            Assert.False(updated.IsDefault);

            // Verify recurring transactions are removed.
            var rTransactions =
                this.RecurringTransactionManager.GetRecurringTransactionsByFilter(
                    Maybe<TransactionType>.None, account.Id, Maybe<int>.None, true);
            Assert.Empty(rTransactions);

            this.AccountManager.SetAccountObsolete(account.Id, false);
            updated = this.AccountManager.GetAccount(account.Id);

            // Verify account is no longer obsolete
            Assert.False(updated.IsObsolete);
            Assert.False(updated.IsDefault);
        }

        /// <summary>
        /// Tests the exceptional flow of the SetAccountObsolete method.
        /// </summary>
        [Fact]
        public void SetAccountObsolete_Exceptions()
        {
            var account = this.GenerateAccount("Description");

            var transaction = this.GenerateTransaction(accountId: account.Id, amount: -50);

            // Account has current balance of -50.
            Assert.Throws<ValidationException>(() => this.AccountManager.SetAccountObsolete(account.Id, true));

            var transaction2 = this.GenerateTransaction(accountId: account.Id, type: TransactionType.External, amount: 50);

            this.AccountManager.SetAccountObsolete(account.Id, true);

            // Create account with same description as inactive account.
            var account2 = this.GenerateAccount("Description");

            Assert.Throws<ValidationException>(() => this.AccountManager.SetAccountObsolete(account.Id, false));
        }

        #endregion SetAccountObsolete
    }
}
