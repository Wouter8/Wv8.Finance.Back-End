namespace PersonalFinance.Common.DataTransfer.Input
{
    using Wv8.Core;

    /// <summary>
    /// A class containing the user input to create or edit a payment request with.
    /// </summary>
    public class InputPaymentRequest
    {
        /// <summary>
        /// The identifier of the payment request. If <c>None</c>, it is a new payment request.
        /// </summary>
        public Maybe<int> Id { get; set; }

        /// <summary>
        /// The amount that is due.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The name of the group/person to receive money from.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The amount of payments that are requested.
        /// </summary>
        public int Count { get; set; }
    }
}