namespace PersonalFinance.Business.Category
{
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Interface for the manager providing functionality related to categories.
    /// </summary>
    public interface ICategoryManager
    {
        /// <summary>
        /// Retrieves an category based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <returns>The category.</returns>
        Category GetCategory(int id);

        /// <summary>
        /// Retrieves categories from the database.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete categories should also be retrieved.</param>
        /// <returns>The list of categories.</returns>
        List<Category> GetCategories(bool includeObsolete);

        /// <summary>
        /// Retrieves categories from the database with a specified filter.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete categories should also be retrieved.</param>
        /// <param name="type">The type of categories to retrieve.</param>
        /// <returns>The list of filtered categories.</returns>
        List<Category> GetCategoriesByFilter(bool includeObsolete, CategoryType type);

        /// <summary>
        /// Updates an category.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <param name="description">The new description of the category.</param>
        /// <param name="type">The new type of the category.</param>
        /// <param name="parentCategoryId">Optionally, the identifier of the new parent category.</param>
        /// <param name="iconPack">The new icon pack of the icon for the category.</param>
        /// <param name="iconName">The new name of the icon for the category.</param>
        /// <param name="iconColor">The new background color of the icon for the category.</param>
        /// <returns>The updated category.</returns>
        Category UpdateCategory(int id, string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor);

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="description">The description of the category.</param>
        /// <param name="type">The type of the category.</param>
        /// <param name="parentCategoryId">Optionally, the identifier of the parent category.</param>
        /// <param name="iconPack">The icon pack of the icon for the category.</param>
        /// <param name="iconName">The name of the icon for the category.</param>
        /// <param name="iconColor">The background color of the icon for the category.</param>
        /// <returns>The created category.</returns>
        Category CreateCategory(string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor);

        /// <summary>
        /// Sets the obsolete value of an category.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <param name="obsolete">The new obsolete value for the category.</param>
        /// <remarks>Nothing happens if the existing obsolete value is the same as the provided obsolete value.</remarks>
        void SetCategoryObsolete(int id, bool obsolete);
    }
}