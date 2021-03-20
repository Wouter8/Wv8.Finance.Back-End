namespace PersonalFinance.Data.Models
{
    using System.Linq;
    using NodaTime;
    using Wv8.Core;

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

        /// <summary>
        /// Get the amount of the transaction that is personally due. This can be different from the amount on the
        /// transaction when that amount contains an amount paid for others or paid by others. These differences are
        /// stored in the linked split details or payment request.
        /// </summary>
        /// <returns>The personal amount of the transaction.</returns>
        public decimal PersonalAmount =>
            this.Amount
            + this.PaymentRequests.Sum(pr => pr.Count * pr.Amount)
            // When I paid for others, then subtract the amount paid for others.
            // When someone else paid for me, then add that share to the personal amount.
            + (this.SplitwiseTransaction.ToMaybe().Select(st =>
                st.OwedToOthers - st.OwedByOthers).ValueOrElse(0) * -1);

        /// <summary>
        /// Indicates whether or not the transaction can be edited within this application.
        /// This can be false if the transaction is imported from Splitwise and someone else paid for it, the
        /// transaction should then be updated in Splitwise.
        /// Note that the category of a transaction can always be changed, since this is only used internally.
        /// </summary>
        public bool FullyEditable =>
            this.SplitwiseTransaction.ToMaybe().Select(t => t.PaidAmount > 0).ValueOrElse(true);
    }
}