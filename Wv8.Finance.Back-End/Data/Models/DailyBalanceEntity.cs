namespace PersonalFinance.Data.Models
{
    using NodaTime;

    /// <summary>
    /// A historical entity representing an account. Used for different bank accounts, etc.
    /// </summary>
    public class DailyBalanceEntity
    {
        /// <summary>
        /// The identifier of the account.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// The account entity.
        /// </summary>
        public AccountEntity Account { get; set; }

        /// <summary>
        /// The date this entity belongs to.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// The balance of <see cref="Account"/> on <see cref="Date"/>.
        /// </summary>
        public decimal Balance { get; set; }
    }
}