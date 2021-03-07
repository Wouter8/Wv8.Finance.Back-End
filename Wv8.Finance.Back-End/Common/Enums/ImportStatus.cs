namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enumeration representing the state of an importing process.
    /// </summary>
    public enum ImportState
    {
        /// <summary>
        /// Indicates that the importer is not running and that the last run was successful.
        /// </summary>
        NotRunning = 0,

        /// <summary>
        /// Indicates that the importer is currently running.
        /// </summary>
        Running = 1,
    }
}