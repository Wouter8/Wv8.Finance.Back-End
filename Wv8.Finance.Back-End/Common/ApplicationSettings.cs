﻿namespace PersonalFinance.Common
{
    /// <summary>
    /// A class containing the application settings.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// The base url for the Splitwise API.
        /// </summary>
        public string SplitwiseRootUrl { get; set; }

        /// <summary>
        /// The API key for the Splitwise API.
        /// </summary>
        public string SplitwiseApiKey { get; set; }
    }
}