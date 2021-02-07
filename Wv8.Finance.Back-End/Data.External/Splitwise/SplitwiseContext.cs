namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.Extensions.Options;
    using PersonalFinance.Common;
    using PersonalFinance.Data.External.Splitwise.Models;
    using PersonalFinance.Data.External.Splitwise.RequestResults;
    using RestSharp;
    using RestSharp.Serializers.NewtonsoftJson;

    /// <summary>
    /// A class containing functionality to communicate with Splitwise.
    /// </summary>
    public class SplitwiseContext : ISplitwiseContext
    {
        /// <summary>
        /// The client to use for HTTP requests.
        /// </summary>
        private readonly IRestClient client;

        /// <summary>
        /// The user id in Splitwise.
        /// </summary>
        private readonly int userId;

        /// <summary>
        /// The group id in Splitwise.
        /// </summary>
        private readonly int groupId;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitwiseContext"/> class.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        public SplitwiseContext(IOptions<ApplicationSettings> settings)
        {
            var baseUrl = settings.Value.SplitwiseRootUrl;
            this.client = new RestClient(baseUrl).UseNewtonsoftJson();

            this.client.AddDefaultHeader("Authorization", $"Bearer {settings.Value.SplitwiseApiKey}");

            this.userId = settings.Value.SplitwiseUserId;
            this.groupId = settings.Value.SplitwiseGroupId;
        }

        /// <inheritdoc/>
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            var updatedAfterString = updatedAfter.ToString("O");
            var limit = 0; // 0 is unlimited.

            var request = new RestRequest("get_expenses", Method.GET);

            request
                .AddParameter("group_id", this.groupId)
                .AddParameter("limit", limit)
                .AddParameter("updated_after", updatedAfterString);

            return this.Execute<GetExpensesResult>(request)
                .Expenses
                // Only return expenses for the user where someone else paid.
                // Expenses where the user paid should be added in the Finance application.
                .Where(e => e.Users.Any(u => u.UserId == this.userId && u.PaidShare > 0))
                .Select(e => e.ToDomainObject(this.userId))
                .ToList();
        }

        /// <summary>
        /// Executes a <see cref="RestRequest"/> and parses the response in type <typeparamref name="T"/>.
        /// <typeparam name="T">The class to parse the response to. Class must have a public empty constructor
        /// so that it can be populated during deserialization.</typeparam>
        /// <param name="request">The request to execute.</param>
        /// <returns>The parsed output as type <typeparamref name="T"/>.</returns>
        /// </summary>
        private T Execute<T>(RestRequest request)
            where T : new()
        {
            var response = this.client.Execute<T>(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new Exception("Application was not allowed to retrieve the requested information from Splitwise.");
            }

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            throw new Exception(
                $"Error while retrieving information from Splitwise: status code was {response.StatusCode}.");
        }
    }
}