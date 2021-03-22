namespace PersonalFinance.Data.Models
{
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// An entity representing a blue print for a transaction which get created based on an interval.
    /// </summary>
    public class RecurringTransactionEntity : BaseTransactionEntity
    {
        /// <summary>
        /// The date from which transactions should be created.
        /// If this is a date in the past, transactions will be created retroactively.
        /// </summary>
        public LocalDate StartDate { get; set; }

        /// <summary>
        /// The inclusive date till which transactions should be created.
        /// If <c>null</c>, then transactions are created indefinitely.
        /// </summary>
        public LocalDate? EndDate { get; set; }

        /// <summary>
        /// The date for the next transaction that will be created from this blueprint.
        /// </summary>
        public LocalDate? NextOccurence { get; set; }

        /// <summary>
        /// The date of the latest transaction that was created from this blueprint.
        /// </summary>
        public LocalDate? LastOccurence { get; set; }

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

        /// <summary>
        /// Get the amount of the transaction that is personally due. This can be different from the amount on the
        /// transaction when that amount contains an amount paid for others or paid by others. These differences are
        /// stored in the linked split details or payment request.
        /// </summary>
        /// <returns>The personal amount of the transaction.</returns>
        public decimal PersonalAmount =>
            this.Amount
            + this.PaymentRequests.Sum(pr => pr.Count * pr.Amount)
            + this.SplitDetails.Sum(sd => sd.Amount);
    }
}