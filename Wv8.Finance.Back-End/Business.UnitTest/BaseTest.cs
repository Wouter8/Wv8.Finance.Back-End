using System;
using Xunit;

namespace Business.UnitTest
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Common;
    using PersonalFinance.Data;

    public abstract class BaseTest
    {
        protected readonly Context Context;

        protected readonly IAccountManager AccountManager;

        protected BaseTest()
        {
            var services = new ServiceCollection();

            services.AddDbContext<Context>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddTransient<IAccountManager, AccountManager>();

            var serviceProvider = services.BuildServiceProvider();

            this.Context = serviceProvider.GetService<Context>();
            this.AccountManager = serviceProvider.GetService<IAccountManager>();
        }

        #region CreateHelpers

        private string GetRandomString(int length = 16)
        {
            return Guid.NewGuid().ToString().Substring(0, length);
        }

        protected Account GenerateAccount(
            string description = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            return this.AccountManager.CreateAccount(
                description ?? this.GetRandomString(),
                iconPack ?? this.GetRandomString(3),
                iconName ?? this.GetRandomString(6),
                iconColor ?? this.GetRandomString(7));
        }

        #endregion CreateHelpers

        #region AssertHelpers

        protected void AssertEqual(Icon a, Icon b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.Color, b.Color);
            Assert.Equal(a.Pack, b.Pack);
        }

        protected void AssertEqual(Account a, Account b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.CurrentBalance, b.CurrentBalance);
            Assert.Equal(a.IsDefault, b.IsDefault);
            Assert.Equal(a.IsObsolete, b.IsObsolete);
            Assert.Equal(a.IconId, b.IconId);
            this.AssertEqual(a.Icon, b.Icon);
        }

        #endregion AssertHelpers
    }
}
