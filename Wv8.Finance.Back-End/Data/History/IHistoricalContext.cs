namespace PersonalFinance.Data.History
{
    using System;

    /// <summary>
    /// An interface for a context which has sets for historical entities.
    /// </summary>
    public interface IHistoricalContext
    {
        /// <summary>
        /// The date and time this context was created. It will always be unique,
        /// potentially with just one tick.
        /// </summary>
        DateTime CreationDateTime { get; set; }
    }
}