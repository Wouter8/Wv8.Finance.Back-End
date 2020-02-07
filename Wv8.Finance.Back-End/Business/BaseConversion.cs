namespace PersonalFinance.Business
{
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.Models;

    /// <summary>
    /// Conversion class for default conversion methods.
    /// </summary>
    public static class BaseConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static Icon AsIcon(this IconEntity entity)
        {
            return new Icon
            {
                Id = entity.Id,
                Name = entity.Name,
                Pack = entity.Pack,
                Color = entity.Color,
            };
        }
    }
}