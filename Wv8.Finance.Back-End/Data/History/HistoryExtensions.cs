namespace PersonalFinance.Data.History
{
    using System;
    using System.Collections.Generic;
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
        /// Returns all days between <paramref name="start"/> and <paramref name="end"/>.
        /// The time of each date is set to 00:00.
        /// </summary>
        /// <param name="start">The start of the period.</param>
        /// <param name="end">The end of the period.</param>
        /// <returns>The list of dates.</returns>
        public static List<DateTime> GetDaysBetween(DateTime start, DateTime end)
        {
            var list = new List<DateTime>();
            if (start > end) return list;

            var date = start;
            while (date <= end)
            {
                list.Add(date.Date);
                date = date.AddDays(1);
            }

            return list;
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

        /// <summary>
        /// Creates a new historical entry for a currently valid entity. The existing entity will be marked invalid.
        /// The new entity will be added to the context.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="entity">The currently valid entity.</param>
        /// <param name="context">The context.</param>
        /// <returns>The newly valid entity.</returns>
        public static T NewHistoricalEntry<T>(this T entity, IHistoricalContext context)
            where T : class, IHistoricalEntity
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (entity.ValidTo < DateTime.MaxValue)
                throw new InvalidOperationException("The specified entity is not currently valid.");

            entity.ValidTo = context.CreationDateTime;

            var newEntity = (T)entity.Clone();
            newEntity.ValidFrom = context.CreationDateTime;
            newEntity.ValidTo = DateTime.MaxValue;

            context.Entry(entity).State = EntityState.Added;

            return newEntity;
        }
    }
}