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

        /// <summary>
        /// A class containing all routes for the category service.
        /// </summary>
        public static class Category
        {
            /// <summary>
            /// The GET route for GetCategory.
            /// Identifier is expected.
            /// </summary>
            public const string GetCategory = "/categories/{id}";

            /// <summary>
            /// The GET route for GetCategories.
            /// </summary>
            public const string GetCategories = "/categories";

            /// <summary>
            /// The GET route for GetCategoriesByFilter.
            /// Multiple query parameters are expected.
            /// </summary>
            public const string GetCategoriesByFilter = "/categories/filter";

            /// <summary>
            /// The PUT route for UpdateCategory.
            /// Identifier is expected.
            /// Multiple query parameters are expected.
            /// </summary>
            public const string UpdateCategory = "/categories/{id}";

            /// <summary>
            /// The POST route for CreateCategory.
            /// Multiple query parameters are expected.
            /// </summary>
            public const string CreateCategory = "/categories";

            /// <summary>
            /// The PUT route for SetCategoryObsolete.
            /// Identifier is expected.
            /// Query parameter is expected.
            /// </summary>
            public const string SetCategoryObsolete = "/categories/obsolete/{id}";
        }
    }
}