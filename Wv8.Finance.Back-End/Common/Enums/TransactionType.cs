namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enum representing the types a transaction can have.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// A transaction that moves money from or to an external account.
        /// </summary>
        External = 1,

        /// <summary>
        /// A transaction that transfers money from one account to another.
        /// </summary>
        Internal = 2,
    }
}