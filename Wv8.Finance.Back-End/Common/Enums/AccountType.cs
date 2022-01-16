namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enum representing the different types an account can have.
    /// </summary>
    public enum AccountType : byte
    {
        /// <summary>
        /// A normal account. Transactions for these accounts are managed by the user.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// An account which integrates with Splitwise. Transactions are imported and a separation is made between
        /// personal amount and amount paid for/by others.
        /// It is not possible to manually add, edit, or delete transactions of accounts with this type.
        /// It is not possible to have multiple active Splitwise accounts at a given time.
        /// </summary>
        Splitwise = 2,
    }
}