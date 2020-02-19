namespace PersonalFinance.Business.Transaction.Processor
{
    /// <summary>
    /// An interface for a periodic transaction processor.
    /// </summary>
    public interface ITransactionProcessor
    {
        /// <summary>
        /// Runs the processor. Processing all transactions and creating instances for recurring objects.
        /// </summary>
        void Run();
    }
}