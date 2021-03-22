namespace PersonalFinance.Common.DataTransfer.Output
{
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// Information about an importer.
    /// </summary>
    public class ImporterInformation
    {
        /// <summary>
        /// The timestamp at which the importer ran successfully last.
        /// </summary>
        public string LastRunTimestamp { get; set; }

        /// <summary>
        /// The current state of the importer.
        /// </summary>
        public ImportState CurrentState { get; set; }
    }
}