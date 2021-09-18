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
        CategoryReport GetCategoryReport(int categoryId, LocalDate start, LocalDate end);
    }
}