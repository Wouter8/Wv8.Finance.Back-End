namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enumeration representing the result of running an importer.
    /// </summary>
    public enum ImportResult
    {
        /// <summary>
        /// The importer has completed.
        /// </summary>
        Completed = 0,

        /// <summary>
        /// The importer was already running.
        /// </summary>
        AlreadyRunning = 1,
    }
}