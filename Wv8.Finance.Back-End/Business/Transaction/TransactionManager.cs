namespace PersonalFinance.Business.Transaction
{
    using System;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Data;
    using Wv8.Core;

    /// <summary>
    /// The manager for functionality related to transactions.
    /// </summary>
    public class TransactionManager : BaseManager, ITransactionManager
    {
        private readonly TransactionValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionManager"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TransactionManager(Context context)
            : base(context)
        {
            this.validator = new TransactionValidator();
        }

        /// <inheritdoc />
        public Transaction GetTransaction(int id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public TransactionGroup GetTransactionsByFilter(
            Maybe<TransactionType> type,
            Maybe<int> categoryId,
            Maybe<string> startDate,
            Maybe<string> endDate,
            int skip,
            int take)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Transaction UpdateTransaction(string description, string date, decimal amount, Maybe<int> categoryId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Transaction CreateTransaction(TransactionType type, string description, string date, decimal amount, Maybe<int> categoryId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void DeleteTransaction(int id)
        {
            throw new NotImplementedException();
        }
    }
}