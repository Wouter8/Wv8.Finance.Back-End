namespace PersonalFinance.Business.Splitwise
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data;
    using PersonalFinance.Data.External.Splitwise;
    using Wv8.Core.EntityFramework;

    /// <summary>
    /// A manager for functionality related to Splitwise.
    /// </summary>
    public class SplitwiseManager : BaseManager, ISplitwiseManager
    {
        /// <summary>
        /// The Splitwise context.
        /// </summary>
        private readonly ISplitwiseContext splitwiseContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitwiseManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="splitwiseContext">The Splitwise context.</param>
        public SplitwiseManager(Context context, ISplitwiseContext splitwiseContext)
            : base(context)
        {
            this.splitwiseContext = splitwiseContext;
        }

        /// <inheritdoc />
        public List<SplitwiseTransaction> GetSplitwiseTransactions(bool includeImported)
        {
            return this.Context.SplitwiseTransactions
                .WhereIf(!includeImported, t => !t.Imported)
                .OrderBy(t => t.Date)
                .AsEnumerable()
                .Select(t => t.AsSplitwiseTransaction())
                .ToList();
        }
    }
}