namespace PersonalFinance.Business.Transaction
{
    /// <summary>
    /// An interface for a period settler.
    /// </summary>
    public interface IPeriodicSettler
    {
        /// <summary>
        /// Runs the settler. Settling all transactions and creating instances for recurring objects.
        /// </summary>
        void Run();
    }
}