namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enumeration containing values indicating what the interval of a report is.
    /// </summary>
    public enum ReportIntervalUnit
    {
        /// <summary>
        /// The report contains data per date.
        /// </summary>
        Days = 1,

        /// <summary>
        /// The report contains data per week.
        /// </summary>
        Weeks = 2,

        /// <summary>
        /// The report contains data per month.
        /// </summary>
        Months = 3,

        /// <summary>
        /// The report contains data per year.
        /// </summary>
        Years = 4,
    }
}