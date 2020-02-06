namespace PersonalFinance.Service
{
    /// <summary>
    /// A class containing all routes.
    /// </summary>
    public static class ApiRoutes
    {
        /// <summary>
        /// A class containing all routes for the account service.
        /// </summary>
        public static class Account
        {
            /// <summary>
            /// The GET route for GetAccount.
            /// Identifier is expected.
            /// </summary>
            public const string GetAccount = "/accounts/{id}";

            /// <summary>
            /// The GET route for GetAccounts.
            /// </summary>
            public const string GetAccounts = "/accounts";

            /// <summary>
            /// The PUT route for UpdateAccount.
            /// Identifier is expected.
            /// Multiple query parameters are expected.
            /// </summary>
            public const string UpdateAccount = "/accounts/{id}";

            /// <summary>
            /// The POST route for CreateAccount.
            /// Multiple query parameters are expected.
            /// </summary>
            public const string CreateAccount = "/accounts";

            /// <summary>
            /// The PUT route for SetAccountObsolete.
            /// Identifier is expected.
            /// Query parameter is expected.
            /// </summary>
            public const string SetAccountObsolete = "/accounts/obsolete/{id}";
        }
    }
}