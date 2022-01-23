namespace Data.External.IntegrationTest
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using PersonalFinance.Common;
    using PersonalFinance.Data.External.Splitwise;
    using Xunit;

    /// <summary>
    /// The base class for all test classes for integration tests.
    /// </summary>
    [Collection("Tests")]
    public class BaseTest
    {
        /// <summary>
        /// The Splitwise context.
        /// </summary>
        protected readonly SplitwiseContext splitwiseContext;

        /// <summary>
        /// The Splitwise settings.
        /// </summary>
        protected readonly SplitwiseSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTest"/> class.
        /// </summary>
        protected BaseTest()
        {
            var applicationSettings = this.GetApplicationSettings();
            this.splitwiseContext = new SplitwiseContext(applicationSettings);
            this.settings = applicationSettings.Value.SplitwiseSettingsMaybe.Value;
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
