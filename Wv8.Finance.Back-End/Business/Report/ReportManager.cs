namespace PersonalFinance.Business.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Reports;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// The manager for functionality related to accounts.
    /// </summary>
    public class ReportManager : BaseManager, IReportManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ReportManager(Context context)
            : base(context)
        { }

        /// <inheritdoc />
        public CurrentDateReport GetCurrentDateReport()
        {
            // 3 week range.
            var today = DateTime.Today.ToLocalDate();
            var firstDate = today.PlusWeeks(-2);
            var lastDate = today.PlusDays(7);

            // Accounts + balances
            var allAccounts = this.Context.Accounts.IncludeAll().ToList();
            // Active accounts and historical balances based on all accounts,
            // because an account might be inactive but have a historical balance which is not 0.
            var accounts = allAccounts.Where(a => !a.IsObsolete).ToList();
            var netWorth = accounts.Sum(a => a.HistoricalBalances.OrderBy(hb => hb.Date).Last().Balance);

            var historicalNetWorth = new Dictionary<LocalDate, decimal>();
            for (var i = 0; i < (lastDate - firstDate).Days; i++)
            {
                var day = firstDate.PlusDays(i);
                var sum = 0m;

                foreach (var account in allAccounts)
                {
                    sum += account.HistoricalBalances
                        .OrderByDescending(h => h.Date)
                        .FirstOrNone(h => h.Date <= day)
                        .Select(h => h.Balance)
                        .ValueOrElse(0);
                }

                historicalNetWorth.Add(day, sum);
            }

            // Transactions: latest, upcoming and unconfirmed
            var allTransactions = this.Context.Transactions
                .IncludeAll()
                .Where(t => t.Date >= firstDate && t.Date <= lastDate)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToList();
            var latestTransactions = allTransactions
                .Where(t => t.Processed) // A processed transaction can never be in the future.
                .Take(5)
                .ToList();
            var upcomingTransactions = allTransactions // TODO: Concat upcoming recurring transaction instances (with flag so they are not selectable in front-end)
                .Where(t => t.Date > today && !t.NeedsConfirmation)
                .OrderBy(t => t.Date)
                .Take(5)
                .ToList();
            var unconfirmedTransactions = allTransactions
                .OrderBy(t => t.Date)
                .Where(t => t.NeedsConfirmation && !t.IsConfirmed.Value)
                .Take(5)
                .ToList();

            // Budgets: currently running
            var budgets = this.Context.Budgets
                .GetBudgets(Maybe<int>.None, today);

            return new CurrentDateReport
            {
                Accounts = accounts.Select(a => a.AsAccount()).ToList(),
                Budgets = budgets.Select(b => b.AsBudget()).ToList(),
                LatestTransactions = latestTransactions.Select(t => t.AsTransaction()).ToList(),
                UpcomingTransactions = upcomingTransactions.Select(t => t.AsTransaction()).ToList(),
                UnconfirmedTransactions = unconfirmedTransactions.Select(t => t.AsTransaction()).ToList(),
                HistoricalBalance = historicalNetWorth.ToDictionary(kv => kv.Key.ToDateString(), kv => kv.Value),
                NetWorth = netWorth,
            };
        }
    }
}