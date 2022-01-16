namespace PersonalFinance.Data.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// An entity representing a category. A transaction belongs to a category.
    /// </summary>
    public class CategoryEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryEntity"/> class.
        /// </summary>
        public CategoryEntity()
        {
            this.Children = new List<CategoryEntity>();
        }

        /// <summary>
        /// The identifier of this account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of this account.
        /// </summary>
        public string Description { get; set; }

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
        /// The expected sum of transactions within a month.
        /// This value is negative for expense categories and positive for income categories.
        /// This value is used to determine future balances without needing to add all (unknown) transactions manually.
        /// </summary>
        public decimal? ExpectedMonthlyAmount { get; set; }

        /// <summary>
        /// The identifier of the icon for this account.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// The icon for this account.
        /// </summary>
        public IconEntity Icon { get; set; }

        /// <summary>
        /// The children of this category. All transactions of a child also belong to the parent.
        /// </summary>
        public IList<CategoryEntity> Children { get; set; }
    }
}