namespace PersonalFinance.Business.Report
{
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
    }
}