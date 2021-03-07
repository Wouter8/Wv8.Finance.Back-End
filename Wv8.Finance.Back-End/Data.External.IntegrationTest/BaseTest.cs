namespace Data.External.IntegrationTest
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using PersonalFinance.Common;
    using PersonalFinance.Data.External.Splitwise;

    /// <summary>
    /// The base class for all test classes for integration tests.
    /// </summary>
    public class BaseTest
    {
        /// <summary>
        /// The Splitwise context.
        /// </summary>
        protected readonly SplitwiseContext splitwiseContext;

        /// <summary>
        /// The application settings.
        /// </summary>
        protected readonly IOptions<ApplicationSettings> settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            this.settings = this.GetApplicationSettings();
            this.splitwiseContext = new SplitwiseContext(this.settings);
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