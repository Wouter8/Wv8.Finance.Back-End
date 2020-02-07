namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enum representing the types a category can have.
    /// </summary>
    public enum CategoryType
    {
        /// <summary>
        /// Transactions in this category are expenses.
        /// </summary>
        Expense = 0,

        /// <summary>
        /// Transactions in this category are income.
        /// </summary>
        Income = 1,
    }
}