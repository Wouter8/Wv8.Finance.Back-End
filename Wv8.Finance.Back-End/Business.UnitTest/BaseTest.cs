using System;
using Xunit;

namespace Business.UnitTest
{
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using Wv8.Core;
    using Xunit.Sdk;

    public abstract class BaseTest
    {
        protected readonly Context Context;

        protected readonly IAccountManager AccountManager;

        protected readonly ICategoryManager CategoryManager;

        protected BaseTest()
        {
            var services = new ServiceCollection();

            services.AddDbContext<Context>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<ICategoryManager, CategoryManager>();

            var serviceProvider = services.BuildServiceProvider();

            this.Context = serviceProvider.GetService<Context>();
            this.AccountManager = serviceProvider.GetService<IAccountManager>();
            this.CategoryManager = serviceProvider.GetService<ICategoryManager>();
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

        protected Category GenerateCategory(
            CategoryType type = CategoryType.Expense,
            string description = null,
            int? parentCategoryId = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            return this.CategoryManager.CreateCategory(
                description ?? this.GetRandomString(),
                type,
                parentCategoryId.ToMaybe(),
                iconPack ?? this.GetRandomString(3),
                iconName ?? this.GetRandomString(6),
                iconColor ?? this.GetRandomString(7));
        }

        protected Category GenerateCategoryWithParent(
            CategoryType type = CategoryType.Expense,
            string description = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            var parent = this.GenerateCategory();
            return this.CategoryManager.CreateCategory(
                description ?? this.GetRandomString(),
                type,
                parent.Id,
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

        protected void AssertEqual(Category a, Category b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.ParentCategoryId, b.ParentCategoryId);
            Assert.Equal(a.IsObsolete, b.IsObsolete);
            Assert.Equal(a.IconId, b.IconId);
            this.AssertEqual(a.ParentCategory, b.ParentCategory);
            this.AssertEqual(a.Icon, b.Icon);
        }

        protected void AssertEqual(Maybe<Category> a, Maybe<Category> b)
        {
            if (a.IsSome != b.IsSome)
                throw new EqualException(a, b);

            if (a.IsNone && b.IsNone) 
                return;

            this.AssertEqual(a.Value, b.Value);
        }

        #endregion AssertHelpers
    }
}
