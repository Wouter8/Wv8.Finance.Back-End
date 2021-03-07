namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enumeration representing the status of an importing process.
    /// </summary>
    public enum ImportStatus
    {
        /// <summary>
        /// Indicates that the importer is not running and that the last run was successful.
        /// </summary>
        NotRunning = 0,

        /// <summary>
        /// Indicates that the importer is currently running.
        /// </summary>
        Running = 1,

        /// <summary>
        /// Indicates that the last run of the importer failed.
        /// </summary>
        Error = 2,
    }
}