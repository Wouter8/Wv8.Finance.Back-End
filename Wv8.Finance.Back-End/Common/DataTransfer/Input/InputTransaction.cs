namespace PersonalFinance.Common.DataTransfer.Input
{
    /// <summary>
    /// A class containing user input with which a transaction can be created or updated.
    /// </summary>
    public class InputTransaction : InputBaseTransaction
    {
        /// <summary>
        /// The date of the transaction.
        /// </summary>
        public string DateString { get; set; }
    }
}