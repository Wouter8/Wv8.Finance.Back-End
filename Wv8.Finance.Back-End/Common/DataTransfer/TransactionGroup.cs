namespace PersonalFinance.Common.DataTransfer
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A class for data transfer objects containing a list of transactions and a summary of its most important values.
    /// </summary>
    public class TransactionGroup
    {
        /// <summary>
        /// The total sum of all the transactions.
        /// </summary>
        public decimal TotalSum { get; set; }

        /// <summary>
        /// The transactions grouped by category.
        /// </summary>
        public Dictionary<Category, List<Transaction>> TransactionsPerCategory { get; set; }

        /// <summary>
        /// The transactions grouped by type.
        /// </summary>
        public Dictionary<TransactionType, List<Transaction>> TransactionsPerType { get; set; }

        /// <summary>
        /// All transactions
        /// </summary>
        public List<Transaction> Transactions { get; set; }
    }
}