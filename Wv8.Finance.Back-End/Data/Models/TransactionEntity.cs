namespace PersonalFinance.Data.Models
{
    using System.Linq;
    using NodaTime;

    /// <summary>
    /// An entity representing a transaction. This can be an income, expense or transfer.
    /// </summary>
    public class TransactionEntity : BaseTransactionEntity
    {
        /// <summary>
        /// The date this transaction occured. Note that this can be in the future.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// A value indicating if this transaction has been processed.
        /// This value will be false for transactions in the future, and true for transactions in the past.
        /// It can be either for transactions on the current date, depending if the process timer has already ticked.
        /// </summary>
        public bool Processed { get; set; }

        /// <summary>
        /// Optionally, the identifier of the recurring transaction this transaction descended from.
        /// </summary>
        public int? RecurringTransactionId { get; set; }

        /// <summary>
        /// Optionally, the recurring transaction this transaction descended from.
        /// </summary>
        public RecurringTransactionEntity RecurringTransaction { get; set; }

        /// <summary>
        /// A value indicating if this transaction is manually confirmed to be processed.
        /// Only filled if <see cref="BaseTransactionEntity.NeedsConfirmation"/> is true.
        /// </summary>
        public bool? IsConfirmed { get; set; }

        /// <summary>
        /// Optionally, the identifier of the Splitwise transaction this transaction is linked to.
        /// </summary>
        public int? SplitwiseTransactionId { get; set; }

        /// <summary>
        /// Optionally, the Splitwise transaction this transaction is linked to.
        /// </summary>
        public SplitwiseTransactionEntity SplitwiseTransaction { get; set; }
    }
}