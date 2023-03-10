namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;

    /// <summary>
    /// Settings for the period report.
    /// </summary>
    public class PeriodReportData
    {
        /// <summary>
        /// The inclusive start date.
        /// </summary>
        public string Start { get; set; }

        /// <summary>
        /// The inclusive end date.
        /// </summary>
        public string End { get; set; }

        /// <summary>
        /// The category identifiers.
        /// </summary>
        public List<int> CategoryIds { get; set; }
    }
}
