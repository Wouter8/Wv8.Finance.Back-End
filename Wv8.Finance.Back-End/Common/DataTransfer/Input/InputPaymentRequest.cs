namespace PersonalFinance.Common.DataTransfer.Input
{
    /// <summary>
    /// A class containing the user input to create a payment request with.
    /// </summary>
    public class InputPaymentRequest
    {
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