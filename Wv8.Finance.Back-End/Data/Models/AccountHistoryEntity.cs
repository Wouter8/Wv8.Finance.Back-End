namespace PersonalFinance.Data.Models
{
    using System;
    using PersonalFinance.Data.History;

    /// <summary>
    /// A historical entity representing an account. Used for different bank accounts, etc.
    /// </summary>
    public class AccountHistoryEntity : IHistoricalEntity
    {
        /// <summary>
        /// The identifier of the account.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The account entity.
        /// </summary>
        public AccountEntity Account { get; set; }

        /// <inheritdoc />
        public DateTime ValidFrom { get; set; }

        /// <inheritdoc />
        public DateTime ValidTo { get; set; }

        /// <summary>
        /// The current balance of the account.
        /// </summary>
        public decimal Balance { get; set; }

        /// <inheritdoc />
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}