namespace PersonalFinance.Common.DataTransfer
{
    using System.Collections.Generic;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// A class for data transfer objects containing a list of transactions and a summary of its most important values.
    /// </summary>
    public class TransactionGroup
    {
        /// <summary>
        /// The total amount of transactions that could be retrieved. Not all might have been retrieved
        /// because of pagination parameters.
        /// </summary>
        public int TotalSearchResults { get; set; }

        /// <summary>
        /// The total sum of all the transactions.
        /// </summary>
        public decimal TotalSum { get; set; }

        /// <summary>
        /// The sum of the transactions grouped by category for all income categories.
        /// </summary>
        public Dictionary<int, decimal> SumPerIncomeCategory { get; set; }

        /// <summary>
        /// The sum of the transactions grouped by category for all expense categories.
        /// </summary>
        public Dictionary<int, decimal> SumPerExpenseCategory { get; set; }

        /// <summary>
        /// The transactions grouped by category.
        /// </summary>
        public Dictionary<int, List<Transaction>> TransactionsPerCategory { get; set; }

        /// <summary>
        /// The transactions grouped by type.
        /// </summary>
        public Dictionary<TransactionType, List<Transaction>> TransactionsPerType { get; set; }

        /// <summary>
        /// All transactions.
        /// </summary>
        public List<Transaction> Transactions { get; set; }

        /// <summary>
        /// The list of all categories of the transactions.
        /// </summary>
        public Dictionary<int, Category> Categories { get; set; }
    }
}