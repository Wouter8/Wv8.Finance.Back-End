namespace PersonalFinance.Business.Account
{
    using System;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.History;
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
            if (entity.History == null || !entity.History.Any())
                throw new ArgumentNullException(nameof(entity.History));

            return new Account
            {
                Id = entity.Id,
                Description = entity.Description,
                CurrentBalance = entity.History.SingleAtNow().Balance,
                IsDefault = entity.IsDefault,
                IsObsolete = entity.IsObsolete,
                IconId = entity.IconId,
                Icon = entity.Icon.AsIcon(),
            };
        }
    }
}