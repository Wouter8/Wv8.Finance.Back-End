namespace PersonalFinance.Business.Report
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Business.Shared;
    using PersonalFinance.Business.Transaction;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer.Reports;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Extensions;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// The manager for functionality related to accounts.
    /// </summary>
    public class ReportManager : BaseManager, IReportManager
    {
        private readonly BaseValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public ReportManager(Context context)
            : base(context)
        {
            this.validator = new BaseValidator();
        }

        /// <inheritdoc />
        public CurrentDateReport GetCurrentDateReport()
        {
            // 3 month range.
            var today = DateTime.Today.ToLocalDate();
            var firstDate = today.PlusWeeks(-12);
            var nextWeek = today.PlusDays(7); // Used to retrieve transactions in the future.

            // Accounts + balances
            var allAccounts = this.Context.Accounts.IncludeAll().ToList();
            // Active accounts and historical balances based on all accounts,
            // because an account might be inactive but have a historical balance which is not 0.
            var accounts = allAccounts.Where(a => !a.IsObsolete).ToList();
            var dailyBalances = this.Context.DailyBalances
                .ToList()
                .Within(firstDate, today)
                .ToBalanceIntervals()
                .ToFixedPeriod(firstDate, today)
                .ToDailyIntervals();
            var netWorth = accounts.Sum(a => a.CurrentBalance);

            // Transactions: latest, upcoming and unconfirmed
            var allTransactions = this.Context.Transactions
                .IncludeAll()
                .Where(t => t.Date >= firstDate && t.Date <= nextWeek)
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
                HistoricalBalance = dailyBalances.ToDto(),
                NetWorth = netWorth,
            };
        }

        /// <inheritdoc />
        public CategoryReport GetCategoryReport(int categoryId, string startString, string endString)
        {
            var start = this.validator.DateString(startString, "start");
            var end = this.validator.DateString(endString, "end");

            // Verify category exists.
            this.Context.Categories.GetEntity(categoryId);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 12);

            var transactions = this.Context.Transactions.GetTransactions(categoryId, start, end, false);

            var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();

            var expensePerInterval =
                expenseTransactions.MapTransactionsPerInterval(intervals, ts => ts.Sum(true));
            var incomePerInterval =
                incomeTransactions.MapTransactionsPerInterval(intervals, ts => ts.Sum(true));
            var resultPerInterval = expensePerInterval.All(e => e == 0) || incomePerInterval.All(i => i == 0)
                ? Maybe<List<decimal>>.None
                : expensePerInterval.Select((i, index) => i + incomePerInterval[index]).ToList();

            return new CategoryReport
            {
                Dates = intervals.ToDates().ToDateStrings(),
                Unit = unit,
                Expenses = expensePerInterval,
                Incomes = incomePerInterval,
                Results = resultPerInterval,
            };
        }

        /// <inheritdoc />
        public AccountReport GetAccountReport(int accountId, string startString, string endString)
        {
            var start = this.validator.DateString(startString, "start");
            var end = this.validator.DateString(endString, "end");
            this.validator.Period(start, end, true);

            // Verify account exists
            this.Context.Accounts.GetEntity(accountId);

            var dailyBalances = this.Context.DailyBalances.GetDailyBalances(accountId)
                .Within(start, end)
                .ToBalanceIntervals()
                .ToFixedPeriod(start, end)
                .ToDailyIntervals();

            return new AccountReport
            {
                Unit = ReportIntervalUnit.Days,
                Dates = dailyBalances.Select(hb => hb.Interval).ToList().ToDates().ToDateStrings(),
                Balances = dailyBalances.Select(hb => hb.Balance).ToList(),
            };
        }

        public PeriodReport GetPeriodReport(string startString, string endString)
        {
            var start = this.validator.DateString(startString, "start");
            var end = this.validator.DateString(endString, "end");
            this.validator.Period(start, end, true);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            var dailyBalances = this.Context.DailyBalances
                .ToList()
                .Within(start, end)
                .ToBalanceIntervals()
                .ToFixedPeriod(start, end)
                .ToDailyIntervals();

            // TODO: it probably is better to not always include all related entities and just retrieve them manually or include them explicitly for each use case.
            var transactions = this.Context.Transactions.GetTransactions(Maybe<int>.None, start, end, true);
            var transactionsByInterval = transactions.GroupByInterval(intervals);
            var transactionsByCategory = transactions.GroupByCategory();

            var categories = this.Context.Categories.IncludeAll().ToList();
            var rootCategories = categories
                .Where(c => !c.ParentCategoryId.HasValue)
                .ToDictionary(c => c.Id, c => transactionsByCategory.TryGetList(c.Id).Sum());
            var childCategories = categories
                .Where(c => c.ParentCategoryId.HasValue)
                .ToList()
                .GroupBy(c => c.ParentCategoryId.Value)
                .ToDictionary(g => g.Key, g => g.ToDictionary(
                    c => c.Id, c => transactionsByCategory.TryGetList(c.Id).Sum()));

            return new PeriodReport
            {
                Dates = intervals.ToDates().ToDateStrings(),
                Unit = unit,
                Totals = transactions.Sum(),
                SumsPerInterval = transactionsByInterval.Select(kv => kv.Value.Sum()).ToList(),
                TotalsPerRootCategory = rootCategories,
                TotalsPerChildCategory = childCategories,
                DailyNetWorth = dailyBalances.ToDto(),
                Categories = categories.ToDictionary(c => c.Id, c => c.AsCategory()),
            };
        }
    }
}
