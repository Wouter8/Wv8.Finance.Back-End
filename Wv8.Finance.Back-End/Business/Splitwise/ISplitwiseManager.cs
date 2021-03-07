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
        /// <param name="onlyImportable"><c>true</c> if only transactions that are importable should be included.
        /// A transaction is importable if the paid amount is 0 and the transaction has not yet been imported.
        /// <c>false</c> in all other cases.
        /// if only transactions which have to be completely imported should be returned.</param>
        /// <returns>A list of Splitwise transactions.</returns>
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool onlyImportable);

        /// <summary>
        /// Get all relevant users from Splitwise.
        /// </summary>
        /// <returns>A list of Splitwise users.</returns>
        public List<SplitwiseUser> GetSplitwiseUsers();

        /// <summary>
        /// Imports a Splitwise transaction by specifying a category for the transaction.
        /// </summary>
        /// <param name="splitwiseId">The identifier of the Splitwise transaction.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <returns>The imported transaction.</returns>
        public Transaction CompleteTransactionImport(int splitwiseId, int categoryId);

        /// <summary>
        /// Imports new/updated transactions from Splitwise.
        /// </summary>
        public void ImportFromSplitwise();

        /// <summary>
        /// Gets the information about the Splitwise importer.
        /// </summary>
        /// <returns>Information about the importer.</returns>
        public ImporterInformation GetImporterInformation();
    }
}