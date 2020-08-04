namespace PersonalFinance.Common.DataTransfer.Input
{
    using Wv8.Core;

    /// <summary>
    /// A class containing the user input to update a payment request.
    /// </summary>
    public class EditPaymentRequest : InputPaymentRequest
    {
        /// <summary>
        /// The identifier of the payment request. If <c>None</c>, it is a new payment request.
        /// </summary>
        public Maybe<int> Id { get; set; }
    }
}