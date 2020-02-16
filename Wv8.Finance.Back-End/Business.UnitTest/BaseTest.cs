namespace Business.UnitTest
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using Wv8.Core;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// A class with basic functionality for tests.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// The database context to assert things by manually querying the database.
        /// </summary>
        protected readonly Context Context;

        /// <summary>
        /// The account manager.
        /// </summary>
        protected readonly IAccountManager AccountManager;

        /// <summary>
        /// The category manager.
        /// </summary>
        protected readonly ICategoryManager CategoryManager;

        /// <summary>
        /// The budget manager.
        /// </summary>
        protected readonly IBudgetManager BudgetManager;

        /// <summary>
        /// The transaction manager.
        /// </summary>
        protected readonly ITransactionManager TransactionManager;

        /// <summary>
        /// The periodic settler.
        /// </summary>
        protected readonly ITransactionProcessor PeriodicSettler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            var services = new ServiceCollection();

            services.AddDbContext<Context>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<ICategoryManager, CategoryManager>();
            services.AddTransient<IBudgetManager, BudgetManager>();
            services.AddTransient<ITransactionManager, TransactionManager>();
            services.AddTransient<ITransactionProcessor, TransactionProcessor>();

            var serviceProvider = services.BuildServiceProvider();

            this.Context = serviceProvider.GetService<Context>();
            this.AccountManager = serviceProvider.GetService<IAccountManager>();
            this.CategoryManager = serviceProvider.GetService<ICategoryManager>();
            this.BudgetManager = serviceProvider.GetService<IBudgetManager>();
            this.TransactionManager = serviceProvider.GetService<ITransactionManager>();
            this.PeriodicSettler = serviceProvider.GetService<ITransactionProcessor>();
        }

        #region CreateHelpers

        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
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

        /// <summary>
        /// Creates a category with specified, or random values.
        /// </summary>
        /// <param name="type">The category type.</param>
        /// <param name="description">The description.</param>
        /// <param name="parentCategoryId">The identifier of the parent category.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
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

        /// <summary>
        /// Creates a category with a parent.
        /// </summary>
        /// <param name="type">The type of category.</param>
        /// <param name="description">The description of the child.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The create child category.</returns>
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

        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="categoryId">The identifier of a category.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>The created budget.</returns>
        protected Budget GenerateBudget(
            int? categoryId = null,
            decimal? amount = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (!categoryId.HasValue)
                categoryId = this.GenerateCategory().Id;
            if (!startDate.HasValue)
                startDate = DateTime.Today;
            if (!endDate.HasValue)
                endDate = DateTime.Today.AddMonths(1);

            return this.BudgetManager.CreateBudget(
                categoryId.Value,
                amount ?? 100,
                startDate.Value.ToString("O"),
                endDate.Value.ToString("O"));
        }

        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="accountId">The identifier of the account.</param>
        /// <param name="type">The type of the transaction.</param>
        /// <param name="description">The description of the transaction.</param>
        /// <param name="date">The date of the transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="receivingAccountId">The identifier of the receiving account.</param>
        /// <returns>The created transaction.</returns>
        protected Transaction GenerateTransaction(
            int? accountId = null,
            TransactionType type = TransactionType.Expense,
            string description = null,
            DateTime? date = null,
            decimal? amount = null,
            int? categoryId = null,
            int? receivingAccountId = null)
        {
            if (((type == TransactionType.Expense) || (type == TransactionType.Income)) && !categoryId.HasValue)
                categoryId = this.GenerateCategory().Id;
            if (type == TransactionType.Transfer && !receivingAccountId.HasValue)
                receivingAccountId = this.GenerateAccount().Id;

            if (!accountId.HasValue)
                accountId = this.GenerateAccount().Id;
            if (!date.HasValue)
                date = DateTime.Today;

            return this.TransactionManager.CreateTransaction(
                accountId.Value,
                type,
                description ?? this.GetRandomString(),
                date.Value.ToString("O"),
                amount ?? (type == TransactionType.Expense ? -50 : 50),
                categoryId.ToMaybe(),
                receivingAccountId.ToMaybe());
        }

        #endregion CreateHelpers

        #region AssertHelpers

        /// <summary>
        /// Asserts that two icons are the same.
        /// </summary>
        /// <param name="a">Icon a.</param>
        /// <param name="b">Icon b.</param>
        protected void AssertEqual(Icon a, Icon b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Name, b.Name);
            Assert.Equal(a.Color, b.Color);
            Assert.Equal(a.Pack, b.Pack);
        }

        /// <summary>
        /// Asserts that two accounts are the same.
        /// </summary>
        /// <param name="a">Account a.</param>
        /// <param name="b">Account b.</param>
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

        /// <summary>
        /// Asserts that two categories are the same.
        /// </summary>
        /// <param name="a">Category a.</param>
        /// <param name="b">Category b.</param>
        protected void AssertEqual(Category a, Category b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.ParentCategoryId, b.ParentCategoryId);
            Assert.Equal(a.IsObsolete, b.IsObsolete);
            Assert.Equal(a.IconId, b.IconId);
            this.AssertEqual(a.ParentCategory, b.ParentCategory);
            this.AssertEqual(a.Icon, b.Icon);

            foreach (var child in a.Children)
            {
                var bChild = b.Children.Single(c => c.Id == child.Id);
                this.AssertEqual(child, bChild);
            }
        }

        /// <summary>
        /// Asserts that two parent categories are the same.
        /// </summary>
        /// <param name="a">Parent category a.</param>
        /// <param name="b">Parent category b.</param>
        protected void AssertEqual(Maybe<Category> a, Maybe<Category> b)
        {
            if (a.IsSome != b.IsSome)
                throw new EqualException(a, b);

            if (a.IsNone && b.IsNone)
                return;

            this.AssertEqual(a.Value, b.Value);
        }

        /// <summary>
        /// Asserts that two budgets are the same.
        /// </summary>
        /// <param name="a">Budget a.</param>
        /// <param name="b">Budget b.</param>
        protected void AssertEqual(Budget a, Budget b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Amount, b.Amount);
            Assert.Equal(a.Spent, b.Spent);
            Assert.Equal(a.StartDate, b.StartDate);
            Assert.Equal(a.EndDate, b.EndDate);
            Assert.Equal(a.CategoryId, b.CategoryId);
            this.AssertEqual(a.Category, b.Category);
        }

        /// <summary>
        /// Asserts that two transactions are the same.
        /// </summary>
        /// <param name="a">Transaction a.</param>
        /// <param name="b">Transaction b.</param>
        protected void AssertEqual(Transaction a, Transaction b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Amount, b.Amount);
            Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.CategoryId, b.CategoryId);
            Assert.Equal(a.AccountId, b.AccountId);
            Assert.Equal(a.Date, b.Date);
            Assert.Equal(a.ReceivingAccountId, b.ReceivingAccountId);
            Assert.Equal(a.RecurringTransactionId, b.RecurringTransactionId);
        }

        #endregion AssertHelpers

        /// <summary>
        /// Generates a random string.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <returns>The random string.</returns>
        private string GetRandomString(int length = 16)
        {
            return Guid.NewGuid().ToString().Substring(0, length);
        }
    }
}
