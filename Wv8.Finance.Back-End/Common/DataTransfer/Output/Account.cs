namespace PersonalFinance.Common.DataTransfer.Output
{
    using Enums;

    /// <summary>
    /// Data transfer object for an account.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// The identifier of this account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of this account.
        /// </summary>
        public AccountType Type { get; set; }

        /// <summary>
        /// The description of this account.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A value indicating if this account is the default account for new transactions.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The current balance of this account.
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// A value indicating if this account is obsolete. No new transactions can be created for this account.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// The identifier of the icon for this account.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// The icon for this account.
        /// </summary>
        public Icon Icon { get; set; }
    }
}