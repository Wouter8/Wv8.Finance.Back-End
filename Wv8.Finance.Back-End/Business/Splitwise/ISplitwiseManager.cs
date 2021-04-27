namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

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
        /// Imports a Splitwise transaction as a transfer transaction by specifying the sending/receiving account for the transaction.
        /// This can be used when adding a settlement transaction where another user paid the user to settle the balances.
        /// </summary>
        /// <param name="splitwiseId">The identifier of the Splitwise transaction.</param>
        /// <param name="accountId">The sending/receiving account for the transaction.</param>
        /// <returns>The imported transaction.</returns>
        public Transaction CompleteTransferImport(int splitwiseId, int accountId);

        /// <summary>
        /// Imports a Splitwise transaction by specifying a category for the transaction.
        /// </summary>
        /// <param name="splitwiseId">The identifier of the Splitwise transaction.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <param name="accountId">The account identifier for which the transaction must be imported. This is only
        /// relevant if the expense of <paramref name="splitwiseId"/> has been paid for by the user.</param>
        /// <returns>The imported transaction.</returns>
        public Transaction CompleteTransactionImport(int splitwiseId, int categoryId, Maybe<int> accountId);

        /// <summary>
        /// Imports new/updated transactions from Splitwise.
        /// </summary>
        /// <returns>The result of running the importer.</returns>
        public ImportResult ImportFromSplitwise();

        /// <summary>
        /// Gets information about the importer.
        /// </summary>
        /// <returns>Information about the importer.</returns>
        public ImporterInformation GetImporterInformation();
    }
}