namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using Wv8.Core;

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
        /// Imports a Splitwise transaction by specifying a category for the transaction.
        /// </summary>
        /// <param name="splitwiseId">The identifier of the Splitwise transaction.</param>
        /// <param name="accountId">The identifier of the account for which an expense transaction must be made if the
        /// user paid anything for the Splitwise expense.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <returns>The imported transaction.</returns>
        public Transaction ImportTransaction(int splitwiseId, Maybe<int> accountId, int categoryId);

        /// <summary>
        /// Imports new/updated transactions from Splitwise.
        /// </summary>
        public void ImportFromSplitwise();
    }
}