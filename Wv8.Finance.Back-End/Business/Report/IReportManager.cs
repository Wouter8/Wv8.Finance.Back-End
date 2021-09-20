namespace PersonalFinance.Business.Report
{
    using NodaTime;
    using PersonalFinance.Common.DataTransfer.Reports;

    /// <summary>
    /// Interface for the manager providing functionality related to reports.
    /// </summary>
    public interface IReportManager
    {
        /// <summary>
        /// Retrieves the report for the current date.
        /// </summary>
        /// <returns>The report.</returns>
        CurrentDateReport GetCurrentDateReport();

        /// <summary>
        /// Retrieves the report for a specific category.
        /// </summary>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="start">The first date of the report.</param>
        /// <param name="end">The last date of the report.</param>
        /// <returns>The category report.</returns>
        CategoryReport GetCategoryReport(int categoryId, string start, string end);

        /// <summary>
        /// Retrieves the report for a specific account.
        /// </summary>
        /// <param name="accountId">The identifier of the account.</param>
        /// <param name="start">The first date of the report.</param>
        /// <param name="end">The last date of the report.</param>
        /// <returns>The category report.</returns>
        AccountReport GetAccountReport(int accountId, string start, string end);
    }
}