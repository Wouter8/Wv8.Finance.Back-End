namespace PersonalFinance.Data.Models
{
    using System;

    /// <summary>
    /// A class for an entity representing times relevant to synchronization processes.
    /// </summary>
    public class SynchronizationTimesEntity
    {
        /// <summary>
        /// The identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The time at which the Splitwise importer was last ran.
        /// </summary>
        public DateTime SplitwiseLastRun { get; set; }
    }
}