namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;

    /// <summary>
    /// A report for a category.
    /// </summary>
    public class CategoryReport : Report
    {
        /// <summary>
        /// The expenses per interval, same ordering as <see cref="Report.Dates"/>.
        /// </summary>
        public List<decimal> Expenses { get; set; }

        /// <summary>
        /// The income per interval, same ordering as <see cref="Report.Dates"/>.
        /// </summary>
        public List<decimal> Incomes { get; set; }

        /// <summary>
        /// The result (income - expenses) per interval, same ordering as <see cref="Report.Dates"/>.
        /// </summary>
        public List<decimal> Results { get; set; }
    }
}