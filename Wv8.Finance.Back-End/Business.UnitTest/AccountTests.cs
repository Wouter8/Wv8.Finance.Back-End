using System;
using System.Collections.Generic;
using System.Text;

namespace Business.UnitTest
{
    using PersonalFinance.Common;
    using Wv8.Core.Exceptions;
    using Xunit;

    public class AccountTests : BaseTest
    {

        #region GetAccount

        [Fact]
        public void GetAccount()
        {
            var savedAccount = this.AccountManager.CreateAccount("Description", "far", "icon", "#FFFFFF");
            var retrievedAccount = this.AccountManager.GetAccount(savedAccount.Id);

            this.Equal(savedAccount, retrievedAccount);
        }

        [Fact]
        public void GetAccount_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.AccountManager.GetAccount(-1));
        }

        #endregion GetAccount

        #region GetAccounts

        [Fact]
        public void GetAccounts()
        {
            var savedAccounts = new List<Account>();
            savedAccounts.Add(this.AccountManager.CreateAccount("Description1", ""));
        }

        #endregion GetAccounts
    }
}
