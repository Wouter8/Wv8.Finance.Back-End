namespace PersonalFinance.Common.DataTransfer.Output
{
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// An entity representing a blue print for a transaction which get created based on an interval.
    /// </summary>
    public class RecurringTransaction : BaseTransaction
    {
        /// <summary>
        /// The date from which transactions should be created.
        /// If this is a date in the past, transactions will be created retroactively.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// The inclusive date till which transactions should be created.
        /// If <c>None</c>, transactions will be created indefinitely.
        /// </summary>
        public Maybe<string> EndDate { get; set; }

        /// <summary>
        /// The date for the next transaction that will be created from this blueprint.
        /// </summary>
        public Maybe<string> NextOccurence { get; set; }

        /// <summary>
        /// A value indicating if the end date has passed. No more transactions will be created.
        /// </summary>
        public bool Finished { get; set; }

        /// <summary>
        /// The unit in: '<see cref="Interval"/> units.
        /// </summary>
        public IntervalUnit IntervalUnit { get; set; }

        /// <summary>
        /// The x in: 'x <see cref="IntervalUnit"/>.
        /// </summary>
        public int Interval { get; set; }
    }
}