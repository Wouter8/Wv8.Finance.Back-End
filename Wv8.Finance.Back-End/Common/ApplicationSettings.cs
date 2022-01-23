namespace PersonalFinance.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Wv8.Core;

    /// <summary>
    /// A class containing the application settings.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// The Splitwise settings. If <c>null</c>, then Splitwise integration is disabled.
        /// </summary>
        /// <remarks>This is not implemented as a <see cref="Maybe"/> because this results in binding issues, use <see cref="SplitwiseSettingsMaybe"/> instead.</remarks>
        ///
        public SplitwiseSettings SplitwiseSettings { get; set; }

        /// <summary>
        /// The Splitwise settings. If <c>None</c>, then Splitwise integration is disabled.
        /// </summary>
        public Maybe<SplitwiseSettings> SplitwiseSettingsMaybe => this.SplitwiseSettings.ToMaybe();
    }

    /// <summary>
    /// The settings used for Splitwise integration.
    /// </summary>
    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:File may only contain a single type",
        Justification = "Group setting classes")]
    public class SplitwiseSettings
    {
        /// <summary>
        /// The base url for the Splitwise API.
        /// </summary>
        public string SplitwiseRootUrl { get; set; }

        /// <summary>
        /// The API key for the Splitwise API.
        /// </summary>
        public string SplitwiseApiKey { get; set; }

        /// <summary>
        /// The id of the user in Splitwise.
        /// </summary>
        public int SplitwiseUserId { get; set; }

        /// <summary>
        /// The id of the group in Splitwise.
        /// </summary>
        public int SplitwiseGroupId { get; set; }
    }
}
