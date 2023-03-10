namespace PersonalFinance.Common.DataTransfer.Reports
{
    /// <summary>
    /// A class containing total values for a set of transactions.
    /// </summary>
    public class TransactionSums
    {
        /// <summary>
        /// The sum of the expense transactions in the set of transactions.
        /// </summary>
        public decimal Expense { get; set; }

        /// <summary>
        /// The sum of the income transactions in the set of transactions.
        /// </summary>
        public decimal Income { get; set; }
    }
}
