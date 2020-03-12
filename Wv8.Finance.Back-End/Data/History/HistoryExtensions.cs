namespace PersonalFinance.Data.History
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// A class providing extension methods related to historical entities.
    /// </summary>
    public static class HistoryExtensions
    {
        /// <summary>
        /// The amount of ticks in the DateTime object last used by a created context.
        /// Used to ensure that no two contexts have the same creation time, and that
        /// a context created after another context always has a later time.
        /// </summary>
        private static long timeTicks = 0;

        /// <summary>
        /// Gets a date time which will always be unique. This can be used for
        /// <see cref="IHistoricalContext.CreationDateTime"/>.
        /// </summary>
        /// <returns>The unique date time.</returns>
        public static DateTime GetUniqueDateTime()
        {
            while (true)
            {
                var time = DateTime.UtcNow;
                var ticks = Interlocked.Read(ref timeTicks);
                var newTicks = time.Ticks > ticks ? time.Ticks : (ticks + 1);
                if (Interlocked.CompareExchange(ref timeTicks, newTicks, ticks) == ticks)
                    return new DateTime(newTicks, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Returns the query which already filters all entities which are not valid for
        /// the current date.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="set">The database set.</param>
        /// <returns>The filtered query.</returns>
        public static IQueryable<T> AtNow<T>(this DbSet<T> set)
            where T : class, IHistoricalEntity
        {
            var now = DateTime.UtcNow;
            return set.At(now);
        }

        /// <summary>
        /// Returns the query which already filters all entities which are not valid for
        /// a given date and time.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="set">The database set.</param>
        /// <param name="dateTime">The date and time for which the entity must be valid.</param>
        /// <returns>The filtered query.</returns>
        public static IQueryable<T> At<T>(this DbSet<T> set, DateTime dateTime)
            where T : class, IHistoricalEntity
        {
            return set
                .Where(e => e.ValidFrom <= dateTime && e.ValidTo >= dateTime);
        }

        /// <summary>
        /// Returns the query which already filters all entities which are not valid in
        /// a given period.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="set">The database set.</param>
        /// <param name="start">The start of the period in which the entity must be valid.</param>
        /// <param name="end">The start of the period in which the entity must be valid.</param>
        /// <returns>The filtered query.</returns>
        public static IQueryable<T> Between<T>(this DbSet<T> set, DateTime start, DateTime end)
            where T : class, IHistoricalEntity
        {
            return set
                .Where(e =>
                    // Entity is valid across whole period
                    (e.ValidFrom <= start && e.ValidTo >= end) ||
                    // Entity is valid at start, and becomes invalid in period
                    (e.ValidFrom <= start && e.ValidTo >= start && e.ValidTo <= end) ||
                    // Entity is valid not valid at start, but becomes valid in period
                    (e.ValidFrom >= start && e.ValidFrom <= start && e.ValidTo >= end));
        }
    }
}