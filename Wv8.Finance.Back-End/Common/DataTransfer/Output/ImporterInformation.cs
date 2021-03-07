namespace PersonalFinance.Common.DataTransfer.Output
{
    using System;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A class representing information about an importer.
    /// </summary>
    public class ImporterInformation
    {
        /// <summary>
        /// The current status of the importer.
        /// </summary>
        public ImportStatus Status { get; set; }

        /// <summary>
        /// The timestamp at which the importer completed its' last run.
        /// </summary>
        public DateTime LastRun { get; set; }
    }
}