namespace Business.UnitTest.Integration.Helpers
{
    using System;
    using System.Runtime.InteropServices;
    using Business.UnitTest.Integration.Mocks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.SqlServer.NodaTime.Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common;
    using PersonalFinance.Data;
    using PersonalFinance.Data.External.Splitwise;

    /// <summary>
    /// A fixture used to initialize the service provider once for all test classes.
    /// </summary>
    public class ServiceProviderFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceProviderFixture"/> class.
        /// </summary>
        public ServiceProviderFixture()
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

            this.ServiceProvider = services.BuildServiceProvider();

            var context = this.ServiceProvider.GetService<Context>();

            context?.Database.EnsureDeleted();
            context?.Database.Migrate();
        }

        /// <summary>
        /// The service provider collection.
        /// </summary>
        public ServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            this.ServiceProvider?.Dispose();
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
    }
}