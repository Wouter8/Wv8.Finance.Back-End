namespace PersonalFinance.Service.Controllers
{
    using System;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Account;
    using PersonalFinance.Common.DataTransfer.Reports;

    /// <summary>
    /// Service endpoint for actions related to reports.
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        // TODO: Replace with reports manager.
        private readonly IAccountManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ReportController(IAccountManager manager)
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
            throw new NotImplementedException();
        }
    }
}