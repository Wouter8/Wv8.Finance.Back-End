namespace PersonalFinance.Business.Transaction
{
    using System;
    using System.Linq;
    using PersonalFinance.Data;
    using PersonalFinance.Data.Extensions;

    /// <summary>
    /// A class which handles transactions and recurring objects.
    /// </summary>
    public class PeriodicSettler : BaseManager, IPeriodicSettler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicSettler"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public PeriodicSettler(Context context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public void Run()
        {
            this.ConcurrentInvoke(() =>
            {
                this.SettleTransactions();
                this.SettleRecurringBudgets();
                this.SettleRecurringTransactions();

                this.Context.SaveChanges();
            });
        }

        /// <summary>
        /// Settles the transactions that are in the past.
        /// </summary>
        private void SettleTransactions()
        {
            var allTransactions = this.Context.Transactions.ToList();
            var transactionsToBeSettled = this.Context.Transactions
                .IncludeAll()
                .Where(t => !t.Settled && t.Date <= DateTime.Today)
                .ToList();

            foreach (var transaction in transactionsToBeSettled)
            {
                transaction.SettleTransaction(this.Context);
            }
        }

        /// <summary>
        /// Settles the recurring transactions that have to be created.
        /// </summary>
        private void SettleRecurringTransactions()
        {
            // TODO: Implement
        }

        /// <summary>
        /// Settles the recurring budgets that have to be created.
        /// </summary>
        private void SettleRecurringBudgets()
        {
            // TODO: Implement
        }
    }
}