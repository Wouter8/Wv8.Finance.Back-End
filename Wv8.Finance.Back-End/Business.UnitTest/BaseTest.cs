namespace Business.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Business.UnitTest.Mocks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NodaTime;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.Processor;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Input;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.External.Splitwise;
    using Wv8.Core;
    using Xunit;
    using Xunit.Sdk;

    /// <summary>
    /// A class with basic functionality for tests.
    /// </summary>
    [Collection("Tests")]
    public abstract class BaseTest : IDisposable
    {
        /// <summary>
        /// The database context to assert things by manually querying the database.
        /// </summary>
        protected Context context;

        /// <summary>
        /// The service provider to retrieve services/managers.
        /// </summary>
        private readonly ServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            var services = new ServiceCollection();

            // Settings
            services.AddTransient(_ => this.GetApplicationSettings());

            // Database context
            services.AddDbContext<Context>(
                options => options.UseSqlServer(
                    this.GetDatabaseConnectionString(),
                    sqlOptions => sqlOptions.UseNodaTime()),
                ServiceLifetime.Transient);

            // Managers
            services.AddTransient<IAccountManager, AccountManager>();
            services.AddTransient<ICategoryManager, CategoryManager>();
            services.AddTransient<IBudgetManager, BudgetManager>();
            services.AddTransient<ITransactionManager, TransactionManager>();
            services.AddTransient<IRecurringTransactionManager, RecurringTransactionManager>();
            services.AddTransient<IReportManager, ReportManager>();
            services.AddTransient<ISplitwiseManager, SplitwiseManager>();

            // Mocks
            services.AddSingleton<ISplitwiseContext, SplitwiseContextMock>();

            this.serviceProvider = services.BuildServiceProvider();

            this.RefreshContext();
            this.context.Database.EnsureDeleted();
            this.context.Database.Migrate();
        }

        /// <summary>
        /// The account manager.
        /// </summary>
        protected IAccountManager AccountManager => this.serviceProvider.GetService<IAccountManager>();

        /// <summary>
        /// The category manager.
        /// </summary>
        protected ICategoryManager CategoryManager => this.serviceProvider.GetService<ICategoryManager>();

        /// <summary>
        /// The budget manager.
        /// </summary>
        protected IBudgetManager BudgetManager => this.serviceProvider.GetService<IBudgetManager>();

        /// <summary>
        /// The transaction manager.
        /// </summary>
        protected ITransactionManager TransactionManager => this.serviceProvider.GetService<ITransactionManager>();

        /// <summary>
        /// The transaction manager.
        /// </summary>
        protected IRecurringTransactionManager RecurringTransactionManager => this.serviceProvider.GetService<IRecurringTransactionManager>();

        /// <summary>
        /// The transaction manager.
        /// </summary>
        protected IReportManager ReportManager => this.serviceProvider.GetService<IReportManager>();

        /// <summary>
        /// The Splitwise manager.
        /// </summary>
        protected ISplitwiseManager SplitwiseManager => this.serviceProvider.GetService<ISplitwiseManager>();

        /// <summary>
        /// The transaction processor.
        /// </summary>
        protected TransactionProcessor TransactionProcessor => new (this.context, this.SplitwiseContextMock);

        /// <summary>
        /// The Splitwise context mock.
        /// </summary>
        protected SplitwiseContextMock SplitwiseContextMock => (SplitwiseContextMock)this.serviceProvider.GetService<ISplitwiseContext>();

        /// <inheritdoc />
        public void Dispose()
        {
            this.serviceProvider?.Dispose();
        }

        #region InputHelpers

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
        /// <param name="needsConfirmation">A value indicating if the transaction has to be confirmed.</param>
        /// <param name="paymentRequests">The payment requests of the transaction.</param>
        /// <param name="splitwiseSplits">The Splitwise splits of the transaction.</param>
        /// <returns>The created transaction.</returns>
        protected InputTransaction GetInputTransaction(
            int accountId,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? date = null,
            decimal? amount = null,
            int? categoryId = null,
            int? receivingAccountId = null,
            bool needsConfirmation = false,
            List<InputPaymentRequest> paymentRequests = null,
            List<InputSplitwiseSplit> splitwiseSplits = null)
        {
            if ((type == TransactionType.Expense || type == TransactionType.Income) && !categoryId.HasValue)
                throw new Exception("Specify a category for an income or expense transaction.");
            if (type == TransactionType.Transfer && !receivingAccountId.HasValue)
                throw new Exception("Specify a receiving account for a transfer transaction.");

            return new InputTransaction
            {
                AccountId = accountId,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? this.GetRandomString(),
                DateString = date.ToMaybe().ValueOrElse(DateTime.Now.ToLocalDate()).ToDateString(),
                CategoryId = categoryId.ToMaybe(),
                ReceivingAccountId = receivingAccountId.ToMaybe(),
                NeedsConfirmation = needsConfirmation,
                PaymentRequests = paymentRequests ?? new List<InputPaymentRequest>(),
                SplitwiseSplits = splitwiseSplits ?? new List<InputSplitwiseSplit>(),
            };
        }

        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="accountId">The identifier of the account.</param>
        /// <param name="type">The type of the recurring transaction.</param>
        /// <param name="description">The description of the recurring transaction.</param>
        /// <param name="startDate">The start date of the recurring transaction.</param>
        /// <param name="endDate">The end date of the recurring transaction.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="receivingAccountId">The identifier of the receiving account.</param>
        /// <param name="needsConfirmation">The value for needs confirmation.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="intervalUnit">The interval unit.</param>
        /// <param name="paymentRequests">The payment requests of the transaction.</param>
        /// <param name="splitwiseSplits">The Splitwise splits of the transaction.</param>
        /// <returns>The created transaction.</returns>
        protected InputRecurringTransaction GetInputRecurringTransaction(
            int accountId,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? startDate = null,
            LocalDate? endDate = null,
            decimal? amount = null,
            int? categoryId = null,
            int? receivingAccountId = null,
            bool needsConfirmation = false,
            int interval = 1,
            IntervalUnit intervalUnit = IntervalUnit.Weeks,
            List<InputPaymentRequest> paymentRequests = null,
            List<InputSplitwiseSplit> splitwiseSplits = null)
        {
            if ((type == TransactionType.Expense || type == TransactionType.Income) && !categoryId.HasValue)
                throw new Exception("Specify a category for an income or expense transaction.");
            if (type == TransactionType.Transfer && !receivingAccountId.HasValue)
                throw new Exception("Specify a receiving account for a transfer transaction.");

            return new InputRecurringTransaction
            {
                AccountId = accountId,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? this.GetRandomString(),
                StartDateString = startDate.ToMaybe().ValueOrElse(DateTime.Now.ToLocalDate()).ToDateString(),
                EndDateString = endDate.ToMaybe().Select(d => d.ToDateString()),
                CategoryId = categoryId.ToMaybe(),
                ReceivingAccountId = receivingAccountId.ToMaybe(),
                NeedsConfirmation = needsConfirmation,
                Interval = interval,
                IntervalUnit = intervalUnit,
                PaymentRequests = paymentRequests ?? new List<InputPaymentRequest>(),
                SplitwiseSplits = splitwiseSplits ?? new List<InputSplitwiseSplit>(),
            };
        }

        #endregion InputHelpers

        #region CreateHelpers

        /// <summary>
        /// Creates an account with specified, or random values.
        /// </summary>
        /// <param name="type">The account type.</param>
        /// <param name="description">The description.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
        protected Account GenerateAccount(
            AccountType type = AccountType.Normal,
            string description = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            return this.AccountManager.CreateAccount(
                type,
                description ?? this.GetRandomString(),
                iconPack ?? this.GetRandomString(3),
                iconName ?? this.GetRandomString(6),
                iconColor ?? this.GetRandomString(7));
        }

        /// <summary>
        /// Creates a category with specified, or random values.
        /// </summary>
        /// <param name="expectedMonthlyAmount">The expected monthly amount.</param>
        /// <param name="description">The description.</param>
        /// <param name="parentCategoryId">The identifier of the parent category.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The created account.</returns>
        protected Category GenerateCategory(
            decimal? expectedMonthlyAmount = null,
            string description = null,
            int? parentCategoryId = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            return this.CategoryManager.CreateCategory(
                description ?? this.GetRandomString(),
                expectedMonthlyAmount ?? Maybe<decimal>.None,
                parentCategoryId.ToMaybe(),
                iconPack ?? this.GetRandomString(3),
                iconName ?? this.GetRandomString(6),
                iconColor ?? this.GetRandomString(7));
        }

        /// <summary>
        /// Creates a category with a parent.
        /// </summary>
        /// <param name="expectedMonthlyAmount">The expected monthly amount.</param>
        /// <param name="description">The description of the child.</param>
        /// <param name="iconPack">The icon pack.</param>
        /// <param name="iconName">The icon name.</param>
        /// <param name="iconColor">The icon color.</param>
        /// <returns>The create child category.</returns>
        protected Category GenerateCategoryWithParent(
            decimal? expectedMonthlyAmount = null,
            string description = null,
            string iconPack = null,
            string iconName = null,
            string iconColor = null)
        {
            var parent = this.GenerateCategory();
            return this.CategoryManager.CreateCategory(
                description ?? this.GetRandomString(),
                expectedMonthlyAmount ?? -50m,
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
            LocalDate? startDate = null,
            LocalDate? endDate = null)
        {
            if (!categoryId.HasValue)
                categoryId = this.GenerateCategory().Id;
            if (!startDate.HasValue)
                startDate = LocalDate.FromDateTime(DateTime.Today);
            if (!endDate.HasValue)
                endDate = LocalDate.FromDateTime(DateTime.Today.AddMonths(1));

            return this.BudgetManager.CreateBudget(
                categoryId.Value,
                amount ?? 100,
                startDate.Value.ToDateString(),
                endDate.Value.ToDateString());
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
        /// <param name="needsConfirmation">A value indicating if the transaction has to be confirmed.</param>
        /// <param name="paymentRequests">The payment requests of the transaction.</param>
        /// <param name="splitwiseSplits">The Splitwise splits of the transaction.</param>
        /// <returns>The created transaction.</returns>
        protected Transaction GenerateTransaction(
            int? accountId = null,
            TransactionType type = TransactionType.Expense,
            string description = null,
            LocalDate? date = null,
            decimal? amount = null,
            int? categoryId = null,
            int? receivingAccountId = null,
            bool needsConfirmation = false,
            List<InputPaymentRequest> paymentRequests = null,
            List<InputSplitwiseSplit> splitwiseSplits = null)
        {
            if ((type == TransactionType.Income || type == TransactionType.Expense) && !categoryId.HasValue)
                categoryId = this.GenerateCategory().Id;
            if (type == TransactionType.Transfer && !receivingAccountId.HasValue)
                receivingAccountId = this.GenerateAccount().Id;

            if (!accountId.HasValue)
                accountId = this.GenerateAccount().Id;
            if (!date.HasValue)
                date = LocalDate.FromDateTime(DateTime.Today);

            var input = new InputTransaction
            {
                AccountId = accountId.Value,
                Amount = amount ?? (type == TransactionType.Expense ? -50 : 50),
                Description = description ?? this.GetRandomString(),
                DateString = date.Value.ToDateString(),
                CategoryId = categoryId.ToMaybe(),
                ReceivingAccountId = receivingAccountId.ToMaybe(),
                NeedsConfirmation = needsConfirmation,
                PaymentRequests = paymentRequests ?? new List<InputPaymentRequest>(),
                SplitwiseSplits = splitwiseSplits ?? new List<InputSplitwiseSplit>(),
            };

            return this.TransactionManager.CreateTransaction(input);
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

        /// <summary>
        /// Asserts that two transactions are the same.
        /// </summary>
        /// <param name="a">Transaction a.</param>
        /// <param name="b">Transaction b.</param>
        protected void AssertEqual(RecurringTransaction a, RecurringTransaction b)
        {
            Assert.Equal(a.Id, b.Id);
            Assert.Equal(a.Amount, b.Amount);
            Assert.Equal(a.Description, b.Description);
            Assert.Equal(a.CategoryId, b.CategoryId);
            Assert.Equal(a.AccountId, b.AccountId);
            Assert.Equal(a.StartDate, b.StartDate);
            Assert.Equal(a.EndDate, b.EndDate);
            Assert.Equal(a.ReceivingAccountId, b.ReceivingAccountId);
            Assert.Equal(a.Interval, b.Interval);
            Assert.Equal(a.IntervalUnit, b.IntervalUnit);
            Assert.Equal(a.NextOccurence, b.NextOccurence);
            Assert.Equal(a.Finished, b.Finished);
            Assert.Equal(a.NeedsConfirmation, b.NeedsConfirmation);
        }

        #endregion AssertHelpers

        /// <summary>
        /// Saves the database context and runs the transaction processor.
        /// </summary>
        protected void SaveAndProcess()
        {
            this.context.SaveChanges();
            this.TransactionProcessor.ProcessAll();
        }

        /// <summary>
        /// Refreshes the database context of this test.
        /// </summary>
        protected void RefreshContext()
        {
            this.context = this.serviceProvider.GetService<Context>();
        }

        /// <summary>
        /// Gets the correct database connection string based on the OS.
        /// GitHub Actions uses Linux.
        /// </summary>
        /// <returns>The connections string.</returns>
        private string GetDatabaseConnectionString()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "Server=(LocalDb)\\MSSQLLocalDB;Database=Wv8-Finance-Test;Integrated Security=SSPI;";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "Server=localhost;Database=Wv8-Finance-Test;User Id=SA;Password=localDatabase1;";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "Server=localhost;Database=Wv8-Finance-Test;User Id=SA;Password=localDatabase1;";
            }

            // This should not happen.
            return string.Empty;
        }

        private IOptions<ApplicationSettings> GetApplicationSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build()
                .GetSection("ApplicationSettings");
            var appSettings = new ApplicationSettings();

            config.Bind(appSettings);

            return Options.Create(appSettings);
        }

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
