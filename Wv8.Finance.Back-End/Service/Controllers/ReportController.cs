namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using PersonalFinance.Business.Report;
    using PersonalFinance.Common.DataTransfer.Reports;

    /// <summary>
    /// Service endpoint for actions related to reports.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ReportController(IReportManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves the report for the current date.
        /// </summary>
        /// <returns>The report.</returns>
        [HttpGet("current-date")]
        public CurrentDateReport GetCurrentDateReport()
        {
            return this.manager.GetCurrentDateReport();
        }

        /// <summary>
        /// Retrieves the report for a specific category.
        /// </summary>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="start">The first date of the report.</param>
        /// <param name="end">The last date of the report.</param>
        /// <returns>The category report.</returns>
        [HttpGet("category/{categoryId}")]
        public CategoryReport GetCategoryReport(int categoryId, string start, string end)
        {
            return this.manager.GetCategoryReport(categoryId, start, end);
        }

        /// <summary>
        /// Retrieves the report for a specific account.
        /// </summary>
        /// <param name="accountId">The identifier of the account.</param>
        /// <param name="start">The first date of the report.</param>
        /// <param name="end">The last date of the report.</param>
        /// <returns>The account report.</returns>
        [HttpGet("account/{accountId}")]
        public AccountReport GetAccountReport(int accountId, string start, string end)
        {
            return this.manager.GetAccountReport(accountId, start, end);
        }

        /// <summary>
        /// Retrieves the report for a given period.
        /// </summary>
        /// <param name="start">The first date of the report.</param>
        /// <param name="end">The last date of the report.</param>
        /// <param name="categoryIds">Only include transactions that have one of these category ids, if empty no filter is applied.</param>
        /// <returns>The period report.</returns>
        [HttpGet]
        public PeriodReport GetPeriodReport(string start, string end, List<int> categoryIds)
        {
            return this.manager.GetPeriodReport(start, end, categoryIds);
        }
    }
}
