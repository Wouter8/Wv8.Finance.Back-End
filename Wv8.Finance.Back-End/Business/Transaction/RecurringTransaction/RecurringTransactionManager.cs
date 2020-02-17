namespace PersonalFinance.Business.Transaction.RecurringTransaction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Business.Transaction.RecurringRecurringtransaction;
    using PersonalFinance.Business.Transaction.RecurringTransaction;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.EntityFramework;
    using Wv8.Core.Exceptions;

    /// <summary>
    /// The manager for functionality related to recurring transactions.
    /// </summary>
    public class RecurringTransactionManager : BaseManager, IRecurringTransactionManager
    {
        private readonly RecurringTransactionValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringTransactionManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public RecurringTransactionManager(Context context)
            : base(context)
        {
            this.validator = new RecurringTransactionValidator();
        }

        /// <inheritdoc />
        public RecurringTransaction GetRecurringTransaction(int id)
        {
            return null;
            //return this.Context.RecurringTransactions.GetEntity(id).AsRecurringTransaction();
        }

        /// <inheritdoc />
        public List<RecurringTransaction> GetRecurringTransactionsByFilter(
            Maybe<TransactionType> type,
            Maybe<int> categoryId)
        {
            return this.Context.RecurringTransactions
                //.IncludeAll()
                .WhereIf(type.IsSome, t => t.Type == type.Value)
                .WhereIf(
                    categoryId.IsSome,
                    t => t.CategoryId.HasValue && (t.CategoryId.Value == categoryId.Value ||
                                                   (t.Category.ParentCategoryId.HasValue &&
                                                    t.Category.ParentCategoryId.Value == categoryId.Value)))
                .OrderBy(t => t.NextOccurence)
                .ToList()
                .Select(t => t.AsRecurringTransaction())
                .ToList();
        }

        /// <inheritdoc />
        public RecurringTransaction UpdateRecurringTransaction(int id, int accountId, string description, string startDate, string endDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            // TODO
            return null;
        }

        /// <inheritdoc />
        public RecurringTransaction CreateRecurringTransaction(int accountId, TransactionType type, string description, string startDate, string endDate, decimal amount, Maybe<int> categoryId, Maybe<int> receivingAccountId)
        {
            // TODO
            return null;
        }

        /// <inheritdoc />
        public void DeleteRecurringTransaction(int id, bool deleteInstances)
        {
            // TODO
        }
    }
}