namespace PersonalFinance.Data.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using global::Data.External.Splitwise.RequestResults;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using PersonalFinance.Common;
    using PersonalFinance.Data.Splitwise.DataTransfer;
    using RestSharp;
    using RestSharp.Serializers.NewtonsoftJson;

    public class SplitwiseContext : ISplitwiseContext
    {
        private readonly IRestClient client;

        public SplitwiseContext(IOptions<ApplicationSettings> settings)
        {
            var baseUrl = settings.Value.SplitwiseRootUrl;
            this.client = new RestClient(baseUrl).UseNewtonsoftJson();
            this.client.AddDefaultHeader("Authorization", $"Bearer {settings.Value.SplitwiseApiKey}");
            this.client.ThrowOnAnyError = true;
        }

        /// <inheritdoc/>
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            var updatedAfterString = updatedAfter.ToString("O");
            var limit = 0; // 0 is unlimited.

            var request = new RestRequest("get_expenses", Method.GET);

            request
                .AddParameter("limit", limit)
                .AddParameter("updated_after", updatedAfterString);

            return this.Execute<GetExpensesResult>(request).Expenses;
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