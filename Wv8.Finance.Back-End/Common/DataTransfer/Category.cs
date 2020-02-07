namespace PersonalFinance.Common.DataTransfer
{
    using System.Collections.Generic;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// An entity representing a category. A transaction belongs to a category.
    /// </summary>
    public class Category
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
        public Maybe<int> ParentCategoryId { get; set; }

        /// <summary>
        /// The parent category of this category.
        /// </summary>
        public Maybe<Category> ParentCategory { get; set; }

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
        public Icon Icon { get; set; }

        /// <summary>
        /// The children of this category. All transactions of a child also belong to the parent.
        /// </summary>
        public List<Category> Children { get; set; }
    }
}