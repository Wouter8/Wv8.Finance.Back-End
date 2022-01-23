namespace PersonalFinance.Data.History
{
    using System;
    using Microsoft.EntityFrameworkCore.ChangeTracking;

    /// <summary>
    /// An interface for a context which has sets for historical entities.
    /// </summary>
    public interface IHistoricalContext
    {
        /// <summary>
        /// The date and time this context was created. It will always be unique,
        /// potentially with just one tick.
        /// </summary>
        DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets an entity object for the given entity providing access to information
        /// about the entity and the ability to perform actions on the entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>An entry for the entity.</returns>
        EntityEntry<T> Entry<T>(T entity)
            where T : class;
    }
}