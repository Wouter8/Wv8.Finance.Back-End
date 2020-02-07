namespace PersonalFinance.Business.Category
{
    using System;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.Models;
    using Wv8.Core;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class CategoryConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        public static Category AsCategory(this CategoryEntity entity)
        {
            if (entity.Icon == null)
                throw new ArgumentNullException(nameof(entity.Icon));
            if (entity.ParentCategoryId.HasValue && entity.ParentCategory == null)
                throw new ArgumentNullException(nameof(entity.ParentCategory));

            return new Category
            {
                Id = entity.Id,
                Description = entity.Description,
                Type = entity.Type,
                ParentCategoryId = entity.ParentCategoryId.ToMaybe(),
                ParentCategory = entity.ParentCategory.ToMaybe().Select(pc => pc.AsCategory()),
                IsObsolete = entity.IsObsolete,
                IconId = entity.IconId,
                Icon = entity.Icon.AsIcon(),
            };
        }
    }
}