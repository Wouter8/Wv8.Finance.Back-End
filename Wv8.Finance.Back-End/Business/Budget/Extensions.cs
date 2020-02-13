namespace PersonalFinance.Business.Budget
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;

    /// <summary>
    /// A class containing extension methods related to budgets.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Orders a list of orders based so that parents and children are next to each other.
        /// </summary>
        /// <param name="budgets">The list of budgets to be ordered.</param>
        /// <returns>The ordered list of budgets.</returns>
        public static List<Budget> OrderBudgets(this List<Budget> budgets)
        {
            var parents = budgets.Where(b => b.Category.ParentCategoryId.IsNone).ToList();
            var allChildren = budgets.Where(b => b.Category.ParentCategoryId.IsSome).ToList();

            var ordered = new List<Budget>();

            foreach (var parent in parents)
            {
                var children = allChildren.Where(b => b.Category.ParentCategoryId.Value == parent.CategoryId).ToList();
                allChildren.RemoveAll(b => b.Category.ParentCategoryId.Value == parent.CategoryId);

                ordered.Add(parent);
                ordered.AddRange(children.OrderByDescending(b => b.Amount));
            }

            return ordered.Concat(
                allChildren
                    .OrderBy(b => b.CategoryId)
                    .ThenByDescending(b => b.Amount))
                .ToList();
        }
    }
}