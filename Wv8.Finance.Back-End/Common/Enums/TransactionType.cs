namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enum representing the types a transaction can have.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// A transaction that removes money from an account.
        /// </summary>
        Expense = 1,

        /// <summary>
        /// A transaction that adds money to an account.
        /// </summary>
        Income = 2,

        /// <summary>
        /// A transaction that transfers money from one account to another.
        /// </summary>
        Internal = 3,
    }
}