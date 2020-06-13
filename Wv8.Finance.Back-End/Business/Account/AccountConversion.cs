namespace PersonalFinance.Business.Account
{
    using System;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class AccountConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static Account AsAccount(this AccountEntity entity)
        {
            if (entity.Icon == null)
                throw new ArgumentNullException(nameof(entity.Icon));
            if (entity.HistoricalBalances == null || !entity.HistoricalBalances.Any())
                throw new ArgumentNullException(nameof(entity.HistoricalBalances));

            return new Account
            {
                Id = entity.Id,
                Description = entity.Description,
                CurrentBalance = entity.HistoricalBalances.Last().Balance,
                IsDefault = entity.IsDefault,
                IsObsolete = entity.IsObsolete,
                IconId = entity.IconId,
                Icon = entity.Icon.AsIcon(),
            };
        }
    }
}