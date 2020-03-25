namespace PersonalFinance.Business.Category
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.EntityFramework;

    /// <summary>
    /// Conversion class containing conversion methods.
    /// </summary>
    public static class CategoryConversion
    {
        /// <summary>
        /// Converts the entity to a data transfer object.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="includeObsoleteChilds">Value indicating if obsolete childs should be included.</param>
        /// <returns>The data transfer object.</returns>
        public static Category AsCategory(this CategoryEntity entity, bool includeObsoleteChilds = true)
        {
            return new Category
            {
                Id = entity.Id,
                Description = entity.Description,
                Type = entity.Type,
                ParentCategoryId = entity.ParentCategoryId.ToMaybe(),
                ParentCategory = entity.ParentCategory.ToMaybe().Select(pc => pc.AsCategory(includeObsoleteChilds)),
                IsObsolete = entity.IsObsolete,
                ExpectedMonthlyAmount = entity.ExpectedMonthlyAmount.ToMaybe(),
                IconId = entity.IconId,
                Icon = entity.Icon?.AsIcon(),
                Children = entity.Children
                    .WhereIf(!includeObsoleteChilds, c => !c.IsObsolete)
                    .Select(c => c.AsChildCategory())
                    .ToList(),
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object without its children to prevent infinite loops.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        private static Category AsParentCategory(this CategoryEntity entity)
        {
            return new Category
            {
                Id = entity.Id,
                Description = entity.Description,
                Type = entity.Type,
                ParentCategoryId = entity.ParentCategoryId.ToMaybe(),
                ParentCategory = entity.ParentCategory.ToMaybe().Select(pc => pc.AsParentCategory()),
                IsObsolete = entity.IsObsolete,
                ExpectedMonthlyAmount = entity.ExpectedMonthlyAmount.ToMaybe(),
                IconId = entity.IconId,
                Icon = entity.Icon?.AsIcon(),
                Children = new List<Category>(), // Set to empty to prevent infinite loop.
            };
        }

        /// <summary>
        /// Converts the entity to a data transfer object with its parent not loading its children to prevent infinite loops.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The data transfer object.</returns>
        private static Category AsChildCategory(this CategoryEntity entity)
        {
            return new Category
            {
                Id = entity.Id,
                Description = entity.Description,
                Type = entity.Type,
                ParentCategoryId = entity.ParentCategoryId.ToMaybe(),
                ParentCategory = entity.ParentCategory.ToMaybe().Select(pc => pc.AsParentCategory()),
                IsObsolete = entity.IsObsolete,
                ExpectedMonthlyAmount = entity.ExpectedMonthlyAmount.ToMaybe(),
                IconId = entity.IconId,
                Icon = entity.Icon?.AsIcon(),
                Children = entity.Children.Select(c => c.AsChildCategory()).ToList(),
            };
        }
    }
}