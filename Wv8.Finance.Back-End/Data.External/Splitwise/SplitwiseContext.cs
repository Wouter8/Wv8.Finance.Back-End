namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using Microsoft.Extensions.Options;
    using NodaTime;
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
        /// <c>true</c> if the Splitwise settings are provided, <c>false</c> otherwise.
        /// If this is <c>false</c> then all methods will return an exception.
        /// </summary>
        private readonly bool integrationEnabled;

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
            var splitwiseSettings = settings.Value.SplitwiseSettingsMaybe;
            this.integrationEnabled = splitwiseSettings.IsSome;

            if (this.integrationEnabled)
            {
                var baseUrl = splitwiseSettings.Value.SplitwiseRootUrl;
                this.client = new RestClient(baseUrl).UseNewtonsoftJson();

                this.client.AddDefaultHeader("Authorization", $"Bearer {splitwiseSettings.Value.SplitwiseApiKey}");

                this.userId = splitwiseSettings.Value.SplitwiseUserId;
                this.groupId = splitwiseSettings.Value.SplitwiseGroupId;
            }
        }

        /// <inheritdoc />
        public bool IntegrationEnabled()
        {
            return this.integrationEnabled;
        }

        /// <inheritdoc/>
        public Expense CreateExpense(decimal totalAmount, string description, LocalDate date, List<Split> splits)
        {
            this.VerifyEnabled();

            var utcDate = DateTime.SpecifyKind(date.ToDateTimeUnspecified(), DateTimeKind.Utc);
            var dateString = utcDate.ToString("O");

            var request = new RestRequest("create_expense", Method.POST);

            var totalAmountPositive = Math.Abs(totalAmount);
            var amountSplit = splits.Sum(s => s.Amount);
            var personalAmount = totalAmountPositive - amountSplit;

            request
                .AddParameter("group_id", this.groupId)
                .AddParameter("cost", totalAmountPositive.ToString(CultureInfo.InvariantCulture))
                .AddParameter("description", description)
                .AddParameter("date", dateString)
                // Add the payer information.
                .AddParameter("users__0__user_id", this.userId)
                .AddParameter("users__0__owed_share", personalAmount.ToString(CultureInfo.InvariantCulture))
                .AddParameter("users__0__paid_share", totalAmountPositive.ToString(CultureInfo.InvariantCulture));
            foreach (var item in splits.Select((split, i) => new { i, split }))
            {
                var split = item.split;
                var index = item.i + 1; // The first index is reserved for the payer.

                request
                    .AddParameter($"users__{index}__user_id", split.UserId)
                    .AddParameter($"users__{index}__owed_share", split.Amount.ToString(CultureInfo.InvariantCulture))
                    .AddParameter($"users__{index}__paid_share", 0);
            }

            return this.Execute<CreateExpenseResult>(request)
                .Expenses
                .Single()
                .ToDomainObject(this.userId);
        }

        /// <inheritdoc/>
        public void DeleteExpense(int id)
        {
            this.VerifyEnabled();

            var request = new RestRequest($"delete_expense/{id}", Method.POST);

            this.Execute<VoidResult>(request);
        }

        /// <inheritdoc/>
        public List<Expense> GetExpenses(DateTime updatedAfter)
        {
            this.VerifyEnabled();

            var updatedAfterString = updatedAfter.ToString("O");
            var limit = 0; // 0 is unlimited.

            var request = new RestRequest("get_expenses", Method.GET);

            request
                .AddParameter("group_id", this.groupId)
                .AddParameter("limit", limit)
                .AddParameter("updated_after", updatedAfterString);

            return this.Execute<GetExpensesResult>(request)
                .Expenses
                .Select(e => e.ToDomainObject(this.userId))
                .ToList();
        }

        /// <inheritdoc/>
        public List<User> GetUsers()
        {
            this.VerifyEnabled();

            var request = new RestRequest("get_group", Method.GET);

            request.AddParameter("id", this.groupId);

            return this.Execute<GetGroupResult>(request)
                .Group
                .Members
                .Where(u => u.Id != this.userId)
                .Select(u => u.ToDomainObject())
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

        private void VerifyEnabled()
        {
            if (!this.integrationEnabled)
                throw new InvalidOperationException("Splitwise integration disabled.");
        }
    }
}
