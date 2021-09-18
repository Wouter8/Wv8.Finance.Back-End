namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;
    using NodaTime;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A base class for a report.
    /// </summary>
    public abstract class Report
    {
        /// <summary>
        /// The dates at which each interval starts.
        /// </summary>
        public List<LocalDate> Dates { get; set; }

        /// <summary>
        /// A value indicating how long each interval is.
        /// </summary>
        public ReportIntervalUnit Unit { get; set; }
    }
}