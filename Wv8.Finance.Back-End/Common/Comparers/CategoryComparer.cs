namespace PersonalFinance.Common.Comparers
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;

    /// <summary>
    /// An equality comparer for two transactions.
    /// </summary>
    public class CategoryComparer : IEqualityComparer<Category>
    {
        /// <inheritdoc />
        public bool Equals(Category x, Category y)
        {
            if ((x == null && y != null) || (y == null && x != null))
                return false;
            if (x == null && y == null)
                return true;
            return x.Id == y.Id;
        }

        /// <inheritdoc />
        public int GetHashCode(Category obj)
        {
            return obj.Id;
        }
    }
}