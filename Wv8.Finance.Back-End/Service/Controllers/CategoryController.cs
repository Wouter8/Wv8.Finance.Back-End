namespace PersonalFinance.Service.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;
    using PersonalFinance.Business.Category;
    using PersonalFinance.Common;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;

    /// <summary>
    /// Service endpoint for actions related to categories.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public CategoryController(ICategoryManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Retrieves an category based on an identifier.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <returns>The category.</returns>
        [HttpGet("{id}")]
        public Category GetCategory(int id)
        {
            return this.manager.GetCategory(id);
        }

        /// <summary>
        /// Retrieves categories from the database.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete categories should also be retrieved.</param>
        /// <returns>The list of categories.</returns>
        [HttpGet]
        public List<Category> GetCategories(bool includeObsolete)
        {
            return this.manager.GetCategories(includeObsolete);
        }

        /// <summary>
        /// Retrieves categories from the database with a specified filter.
        /// </summary>
        /// <param name="includeObsolete">Value indicating if obsolete categories should also be retrieved.</param>
        /// <param name="type">The type of categories to retrieve.</param>
        /// <returns>The list of filtered categories.</returns>
        [HttpGet("filter")]
        public List<Category> GetCategoriesByFilter(bool includeObsolete, CategoryType type)
        {
            return this.manager.GetCategoriesByFilter(includeObsolete, type);
        }

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
        [HttpPut("{id}")]
        public Category UpdateCategory(int id, string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor)
        {
            return this.manager.UpdateCategory(id, description, type, parentCategoryId, iconPack, iconName, iconColor);
        }

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
        [HttpPost]
        public Category CreateCategory(string description, CategoryType type, Maybe<int> parentCategoryId, string iconPack, string iconName, string iconColor)
        {
            return this.manager.CreateCategory(description, type, parentCategoryId, iconPack, iconName, iconColor);
        }

        /// <summary>
        /// Sets the obsolete value of an category.
        /// </summary>
        /// <param name="id">The identifier of the category.</param>
        /// <param name="obsolete">The new obsolete value for the category.</param>
        /// <remarks>Nothing happens if the existing obsolete value is the same as the provided obsolete value.</remarks>
        [HttpPut("obsolete/{id}")]
        public void SetCategoryObsolete(int id, bool obsolete)
        {
            this.manager.SetCategoryObsolete(id, obsolete);
        }
    }
}