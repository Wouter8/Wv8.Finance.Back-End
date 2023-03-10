namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;

    /// <summary>
    /// A class for a report for a single period and no other filters.
    /// </summary>
    public class PeriodReport : Report
    {
        /// <summary>
        /// The net worth per day (key) in the period.
        /// Note that this does not use the same dates as <see cref="Report.Dates"/>.
        /// </summary>
        public Dictionary<string, decimal> DailyNetWorth { get; set; }

        /// <summary>
        /// The totals for the transactions in the period.
        /// </summary>
        public TransactionSums Totals { get; set; }

        /// <summary>
        /// The totals for the transaction in the period, per category identifier, but only for root categories.
        /// Note that entries for categories are only present if they have at least 1 transaction in the period.
        /// </summary>
        public Dictionary<int, TransactionSums> TotalsPerRootCategory { get; set; }

        /// <summary>
        /// The totals per child category. First grouped by parent category identifier and then by child category.
        /// Note that entries for categories are only present if they have at least 1 transaction in the period.
        /// </summary>
        public Dictionary<int, Dictionary<int, TransactionSums>> TotalsPerChildCategory { get; set; }

        /// <summary>
        /// The sum of the transactions in each interval of the period, same ordering as <see cref="Report.Dates"/>.
        /// </summary>
        public List<TransactionSums> SumsPerInterval { get; set; }

        public Dictionary<int, Category> Categories { get; set; }
    }
}
