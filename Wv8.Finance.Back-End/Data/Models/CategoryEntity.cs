﻿namespace PersonalFinance.Data.Models
{
    using PersonalFinance.Common.Enums;

    /// <summary>
    /// An entity representing a category. A transaction belongs to a category.
    /// </summary>
    public class CategoryEntity
    {
        /// <summary>
        /// The identifier of this account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of this account.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Value representing the type of category.
        /// </summary>
        public CategoryType Type { get; set; }

        /// <summary>
        /// The optional identifier of the parent category of this category.
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// The parent category of this category.
        /// </summary>
        public CategoryEntity ParentCategory { get; set; }

        /// <summary>
        /// A value indicating if this account is obsolete. No new transactions can be created for this account.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// The identifier of the icon for this account.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// The icon for this account.
        /// </summary>
        public IconEntity Icon { get; set; }
    }
}