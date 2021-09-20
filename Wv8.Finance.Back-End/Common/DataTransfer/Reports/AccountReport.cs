namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;
    using Wv8.Core;

    /// <summary>
    /// A report for an account.
    /// </summary>
    public class AccountReport : Report
    {
        /// <summary>
        /// The balances per interval, same ordering as <see cref="Report.Dates"/>.
        /// </summary>
        public List<decimal> Balances { get; set; }
    }
}