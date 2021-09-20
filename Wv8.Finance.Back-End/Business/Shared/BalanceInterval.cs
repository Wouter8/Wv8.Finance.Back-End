namespace PersonalFinance.Business.Shared
{
    using NodaTime;

    /// <summary>
    /// A class which contains an interval and a balance which was valid at that interval.
    /// </summary>
    public class BalanceInterval
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BalanceInterval"/> class.
        /// </summary>
        /// <param name="start">The first valid date of the balance.</param>
        /// <param name="end">The last valid date of the balance.</param>
        /// <param name="balance">The balance.</param>
        public BalanceInterval(LocalDate start, LocalDate end, decimal balance)
        {
            this.Interval = new DateInterval(start, end);
            this.Balance = balance;
        }

        /// <summary>
        /// The interval during which the balance is valid.
        /// </summary>
        public DateInterval Interval { get; }

        /// <summary>
        /// The balance.
        /// </summary>
        public decimal Balance { get; }
    }
}