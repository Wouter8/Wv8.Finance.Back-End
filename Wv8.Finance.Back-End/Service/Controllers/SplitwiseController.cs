namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Splitwise;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// Service endpoint for actions related to Splitwise.
    /// </summary>
    [ApiController]
    [Route("api/splitwise")]
    public class SplitwiseController : ControllerBase
    {
        private readonly ISplitwiseManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitwiseController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public SplitwiseController(ISplitwiseManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Get all imported Splitwise transactions. Results are ordered by date.
        /// </summary>
        /// <param name="onlyImportable"><c>true</c> if only transactions that are importable should be included.
        /// A transaction is importable if the paid amount is 0 and the transaction has not yet been imported.
        /// <c>false</c> in all other cases.
        /// if only transactions which have to be completely imported should be returned.</param>
        /// <returns>A list of Splitwise transactions.</returns>
        [HttpGet("transactions")]
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool onlyImportable)
        {
            return this.manager.GetSplitwiseTransactions(onlyImportable);
        }

        /// <summary>
        /// Get all relevant users from Splitwise.
        /// </summary>
        /// <returns>A list of Splitwise users.</returns>
        [HttpGet("users")]
        public List<SplitwiseUser> GetSplitwiseUsers()
        {
            return this.manager.GetSplitwiseUsers();
        }

        /// <summary>
        /// Imports a Splitwise transaction by specifying a category for the transaction.
        /// </summary>
        /// <param name="splitwiseId">The identifier of the Splitwise transaction.</param>
        /// <param name="categoryId">The identifier of the category.</param>
        /// <returns>The imported transaction.</returns>
        [HttpPost("complete-import/{splitwiseId}")]
        public Transaction CompleteTransactionImport(int splitwiseId, int categoryId)
        {
            return this.manager.CompleteTransactionImport(splitwiseId, categoryId);
        }

        /// <summary>
        /// Imports new/updated transactions from Splitwise.
        /// </summary>
        /// <returns>The result of running the importer.</returns>
        [HttpPost("import")]
        public ImportResult ImportFromSplitwise()
        {
            return this.manager.ImportFromSplitwise();
        }

        /// <summary>
        /// Gets information about the importer.
        /// </summary>
        /// <returns>Information about the importer.</returns>
        [HttpGet("importer-information")]
        public ImporterInformation GetImporterInformation()
        {
            return this.manager.GetImporterInformation();
        }
    }
}