namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;

    /// <summary>
    /// A data transfer class containing the report for the current date.
    /// </summary>
    public class CurrentDateReport
    {
        /// <summary>
        /// The latest processed transactions.
        /// </summary>
        public List<Transaction> LatestTransactions { get; set; }

        /// <summary>
        /// The upcoming transactions.
        /// </summary>
        public List<Transaction> UpcomingTransactions { get; set; }

        /// <summary>
        /// Transactions that needs to be confirmed.
        /// </summary>
        public List<Transaction> UnconfirmedTransactions { get; set; }

        /// <summary>
        /// The currently active accounts.
        /// </summary>
        public List<Account> Accounts { get; set; }

        /// <summary>
        /// The current net worth (sum of all account balances).
        /// </summary>
        public decimal NetWorth { get; set; }

        /// <summary>
        /// The historical net worth per date (key).
        /// </summary>
        public Dictionary<string, decimal> HistoricalBalance { get; set; }

        /// <summary>
        /// The currently active budgets.
        /// </summary>
        public List<Budget> Budgets { get; set; }
    }
}