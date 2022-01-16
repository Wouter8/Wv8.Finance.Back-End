namespace PersonalFinance.Common.DataTransfer.Input
{
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// A class containing user input with which a transaction can be created or updated.
    /// </summary>
    public class InputRecurringTransaction : InputBaseTransaction
    {
        /// <summary>
        /// The start date of the recurring transaction.
        /// </summary>
        public string StartDateString { get; set; }

        /// <summary>
        /// The end date of the recurring transaction.
        /// </summary>
        public Maybe<string> EndDateString { get; set; }

        /// <summary>
        /// The interval for this recurring transaction.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The interval unit.
        /// </summary>
        public IntervalUnit IntervalUnit { get; set; }
    }
}