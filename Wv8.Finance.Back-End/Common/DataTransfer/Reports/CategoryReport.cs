namespace PersonalFinance.Common.DataTransfer.Reports
{
    using System.Collections.Generic;
    using Wv8.Core;

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
        /// <c>None</c> when there are either no income or expense transaction for all intervals as the result would
        /// then always equal the value that is present. For example: a salary category probably only has income
        /// transactions, so the result for each interval will just be the sum of the income transactions.
        /// </summary>
        public Maybe<List<decimal>> Results { get; set; }
    }
}