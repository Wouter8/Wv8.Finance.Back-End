namespace PersonalFinance.Common.DataTransfer.Output
{
    /// <summary>
    /// A class for data transfer objects of payment requests.
    /// </summary>
    public class PaymentRequest
    {
        /// <summary>
        /// The identifier of the payment request.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The identifier of the transaction that this payment request belongs to.
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// The amount requested.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The name of the person/group from which money is to be received.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The amount of payments that are requested from the person/group.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The amount of payment requests that are fulfilled.
        /// </summary>
        public int PaidCount { get; set; }

        /// <summary>
        /// The amount that is still due.
        /// </summary>
        public decimal AmountDue { get; set; }

        /// <summary>
        /// <c>true</c> if the payment request is fulfilled, <c>false</c> otherwise.
        /// </summary>
        public bool Complete { get; set; }
    }
}