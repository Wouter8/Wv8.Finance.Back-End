namespace Business.UnitTest.Helpers
{
    using PersonalFinance.Data.External.Splitwise.Models;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// A class containing extension methods for conversion of objects.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Converts a <see cref="SplitDetailEntity"/> to a <see cref="Split"/>.
        /// </summary>
        /// <param name="splitDetailEntity">The entity.</param>
        /// <returns>The created object.</returns>
        public static Split AsSplit(this SplitDetailEntity splitDetailEntity)
        {
            return new Split
            {
                UserId = splitDetailEntity.SplitwiseUserId,
                Amount = splitDetailEntity.Amount,
            };
        }
    }
}