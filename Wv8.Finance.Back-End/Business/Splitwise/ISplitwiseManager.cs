namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;

    /// <summary>
    /// An interface for a manager with functionality related to Splitwise.
    /// </summary>
    public interface ISplitwiseManager
    {
        /// <summary>
        /// Get all imported Splitwise transactions. Results are ordered by date.
        /// </summary>
        /// <param name="includeImported"><c>true</c> if already imported transactions should be included, <c>false</c>
        /// if only transactions which have to be completely imported should be returned.</param>
        /// <returns>A list of Splitwise transactions.</returns>
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool includeImported);

        /// <summary>
        /// Imports new/updated transactions from Splitwise.
        /// </summary>
        public void ImportFromSplitwise();
    }
}