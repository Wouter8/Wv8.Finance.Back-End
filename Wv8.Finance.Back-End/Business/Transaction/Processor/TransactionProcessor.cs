namespace PersonalFinance.Business.Transaction.Processor
{
    using System;
    using System.Linq;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;

    /// <summary>
    /// A class which handles transactions and recurring objects.
    /// </summary>
    public class TransactionProcessor : BaseManager, ITransactionProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionProcessor"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public TransactionProcessor(Context context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public void Run()
        {
            this.ConcurrentInvoke(() =>
            {
                this.ProcessTransactions();
                this.ProcessRecurringBudgets();
                this.ProcessRecurringTransactions();

                this.Context.SaveChanges();
            });
        }

        /// <summary>
        /// Processes the transactions that are in the past.
        /// </summary>
        private void ProcessTransactions()
        {
            var transactionsToBeProcessed = this.Context.Transactions
                .IncludeAll()
                .Where(t => !t.Processed && t.Date <= DateTime.Today &&
                            (!t.NeedsConfirmation || (t.NeedsConfirmation && t.IsConfirmed.Value)))
                .ToList();

            foreach (var transaction in transactionsToBeProcessed)
            {
                transaction.ProcessTransaction(this.Context);
            }
        }

        /// <summary>
        /// Processes the recurring transactions that have to be created.
        /// </summary>
        private void ProcessRecurringTransactions()
        {
            var recurringTransactions = this.Context.RecurringTransactions
                .IncludeAll()
                .Where(rt => !rt.Finished && rt.StartDate <= DateTime.Today)
                .ToList();

            foreach (var recurringTransaction in recurringTransactions)
            {
                recurringTransaction.ProcessRecurringTransaction(this.Context);
            }
        }

        /// <summary>
        /// Processes the recurring budgets that have to be created.
        /// </summary>
        private void ProcessRecurringBudgets()
        {
            // TODO: Implement
        }
    }
}