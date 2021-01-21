namespace PersonalFinance.Business.Splitwise
{
    using PersonalFinance.Common.DataTransfer.Output;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class SplitwiseConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static SplitwiseTransaction AsSplitwiseTransaction(this SplitwiseTransactionEntity entity)
        {
            return new SplitwiseTransaction
            {
                Id = entity.Id,
                Description = entity.Description,
                Date = entity.Date,
                IsDeleted = entity.IsDeleted,
                PaidAmount = entity.PaidAmount,
                PersonalAmount = entity.PersonalAmount,
            };
        }
    }
}