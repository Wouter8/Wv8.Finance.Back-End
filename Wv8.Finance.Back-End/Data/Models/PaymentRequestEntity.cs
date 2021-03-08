namespace PersonalFinance.Data.Models
{
    /// <summary>
    /// A class for payment requests entities in the database.
    /// </summary>
    public class PaymentRequestEntity
    {
        /// <summary>
        /// The identifier of the payment request.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The identifier of the transaction this payment request belongs to.
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// The amount requested.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The name of the person/group from which a payment is requested.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The count of payments that are requested.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The count of payments that have been fulfilled.
        /// </summary>
        public int PaidCount { get; set; }

        /// <summary>
        /// The completed status of this payment requests.
        /// <c>true</c> if <see cref="PaidCount"/> is equal to <see cref="Count"/>.
        /// </summary>
        public bool Completed => this.Count == this.PaidCount;
    }
}