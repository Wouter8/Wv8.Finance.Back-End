namespace Business.UnitTest.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A class containing tests for the category manager.
    /// </summary>
    public class CategoryTests : BaseTest
    {
        #region GetCategory

        /// <summary>
        /// Tests the good flow of the GetCategory method.
        /// </summary>
        [Fact]
        public void GetCategory()
        {
            var savedCategory = this.GenerateCategory();
            var retrievedCategory = this.CategoryManager.GetCategory(savedCategory.Id);

            this.AssertEqual(savedCategory, retrievedCategory);
        }

        /// <summary>
        /// Tests the exceptional flow of the GetCategory method.
        /// </summary>
        [Fact]
        public void GetCategory_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.CategoryManager.GetCategory(-1));
        }

        #endregion GetCategory

        #region GetCategories

        /// <summary>
        /// Tests the good flow of the GetCategories method.
        /// </summary>
        [Fact]
        public void GetCategories()
        {
            // Empty database.
            var retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Empty(retrievedCategories);

            // Create categories.
            const int categoryCount = 5;
            const int childrenCount = 3;
            const int grandChildrenCount = 2;
            const int grandGrandChildrenCount = 1;
            var parentCategories = new List<Category>();
            for (int i = 0; i < categoryCount; i++)
            {
                var parent = this.GenerateCategory();
                for (int j = 0; j < childrenCount; j++)
                {
                    var child = this.GenerateCategory(parentCategoryId: parent.Id);
                    for (int k = 0; k < grandChildrenCount; k++)
                    {
                        var grandChild = this.GenerateCategory(parentCategoryId: child.Id);
                        for (int l = 0; l < grandGrandChildrenCount; l++)
                        {
                            var grandGrandChild = this.GenerateCategory(parentCategoryId: grandChild.Id);
                            grandChild.Children.Add(grandGrandChild);
                        }
                        child.Children.Add(grandChild);
                    }
                    parent.Children.Add(child);
                }
                parentCategories.Add(parent);
            }

            // Load all active categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(false);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Verify categories.
            foreach (var savedCategory in parentCategories)
            {
                var retrievedCategory = retrievedCategories.Single(a => a.Id == savedCategory.Id);
                Assert.Equal(retrievedCategory.Children.Count, childrenCount);

                foreach (var child in retrievedCategory.Children)
                {
                    Assert.Equal(child.Children.Count, grandChildrenCount);
                    foreach (var grandChild in child.Children)
                    {
                        Assert.Equal(grandChild.Children.Count, grandGrandChildrenCount);
                    }
                }
            }

            // Load active and inactive categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Set category obsolete
            this.CategoryManager.SetCategoryObsolete(parentCategories.Last().Id, true);

            // Load all active categories (all except 1)
            retrievedCategories = this.CategoryManager.GetCategories(false);
            Assert.Equal(categoryCount - 1, retrievedCategories.Count);
            Assert.True(retrievedCategories.All(c => c.Id != parentCategories.Last().Id));

            // Load active and inactive categories (should return all again).
            retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Equal(categoryCount, retrievedCategories.Count);
        }

        #endregion GetCategories

        #region GetCategoriesByFilter

        /// <summary>
        /// Tests the good flow of the GetCategoriesByFilter method.
        /// </summary>
        [Fact]
        public void GetCategoriesByFilter()
        {
            var category1 = this.GenerateCategory();
            var category2 = this.GenerateCategory();
            var category3 = this.GenerateCategory(CategoryType.Income);

            this.CategoryManager.SetCategoryObsolete(category2.Id, true);

            var retrievedCategories = this.CategoryManager.GetCategoriesByFilter(true, CategoryType.Expense);
            Assert.Equal(2, retrievedCategories.Count);
            retrievedCategories = this.CategoryManager.GetCategoriesByFilter(false, CategoryType.Expense);
            Assert.Single(retrievedCategories);

            retrievedCategories = this.CategoryManager.GetCategoriesByFilter(true, CategoryType.Income);
            Assert.Single(retrievedCategories);
            retrievedCategories = this.CategoryManager.GetCategoriesByFilter(false, CategoryType.Income);
            Assert.Single(retrievedCategories);
        }

        #endregion GetCategoriesByFilter

        #region UpdateCategory

        /// <summary>
        /// Tests the good flow of the UpdateCategory method.
        /// </summary>
        [Fact]
        public void UpdateCategory()
        {
            var category = this.GenerateCategory();
            var parent = this.GenerateCategory();

            const string newDescription = "Description";
            const CategoryType newType = CategoryType.Expense;
            const string newIconPack = "fas";
            const string newIconName = "circle";
            const string newIconColor = "#FFFFFF";

            var updated = this.CategoryManager.UpdateCategory(category.Id, newDescription, newType, parent.Id, newIconPack, newIconName, newIconColor);

            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newType, updated.Type);
            Assert.Equal(parent.Id, updated.ParentCategoryId);
            Assert.Equal(updated.Icon.Pack, newIconPack);
            Assert.Equal(updated.Icon.Name, newIconName);
            Assert.Equal(updated.Icon.Color, newIconColor);
        }

        /// <summary>
        /// Tests the exceptional flow of the UpdateCategory method.
        /// </summary>
        [Fact]
        public void UpdateCategory_Exceptions()
        {
            var category = this.GenerateCategory();
            var category2 = this.GenerateCategory(description: "Description");

            const string newDescription = "Description";
            const string newIconPack = "fas";
            const string newIconName = "circle";
            const string newIconColor = "#FFFFFF";
            const CategoryType newType = CategoryType.Expense;
            const CategoryType wrongType = CategoryType.Income;
            const string diffDescription = "Description2";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(category.Id, newDescription, newType, Maybe<int>.None, newIconPack, newIconName, newIconColor));

            // Description already exists, but on parent.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(category.Id, newDescription, newType, category2.Id, newIconPack, newIconName, newIconColor));

            // Parent category has different type.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(category.Id, diffDescription, wrongType, category2.Id, newIconPack, newIconName, newIconColor));

            // Category with same description is obsolete.
            this.CategoryManager.SetCategoryObsolete(category2.Id, true);
            this.CategoryManager.UpdateCategory(category.Id, newDescription, newType, Maybe<int>.None, newIconPack, newIconName, newIconColor);

            // Parent category is obsolete.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(category.Id, newDescription, newType, category2.Id, newIconPack, newIconName, newIconColor));

            // Category doesn't exist.
            Assert.Throws<DoesNotExistException>(
                () => this.CategoryManager.UpdateCategory(100, newDescription, newType, Maybe<int>.None, newIconPack, newIconName, newIconColor));
        }

        #endregion UpdateCategory

        #region CreateCategory

        /// <summary>
        /// Tests the good flow of the CreateCategory method.
        /// </summary>
        [Fact]
        public void CreateCategory()
        {
            const string description = "Description";
            const CategoryType type = CategoryType.Expense;
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            var parent = this.CategoryManager.CreateCategory(description, type, Maybe<int>.None, iconPack, iconName, iconColor);

            Assert.Equal(description, parent.Description);
            Assert.Equal(type, parent.Type);
            Assert.Equal(iconPack, parent.Icon.Pack);
            Assert.Equal(iconName, parent.Icon.Name);
            Assert.Equal(iconColor, parent.Icon.Color);
            Assert.False(parent.IsObsolete);
            Assert.False(parent.ParentCategoryId.IsSome);
            Assert.False(parent.ParentCategory.IsSome);

            // Create new category with first category as parent.
            const string description2 = "Description2";
            var child = this.CategoryManager.CreateCategory(description2, type, parent.Id, iconPack, iconName, iconColor);

            Assert.True(child.ParentCategoryId.IsSome);
            Assert.True(child.ParentCategory.IsSome);

            Assert.Equal(parent.Id, child.ParentCategoryId.Value);
        }

        /// <summary>
        /// Tests the exceptional flow of the CreateCategory method.
        /// </summary>
        [Fact]
        public void CreateCategory_Exceptions()
        {
            var parent = this.GenerateCategory(description: "Description");

            const string description = "Description";
            const CategoryType type = CategoryType.Expense;
            const CategoryType diffType = CategoryType.Income;
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(description, type, Maybe<int>.None, iconPack, iconName, iconColor));

            // Parent has different type.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(description, diffType, parent.Id, iconPack, iconName, iconColor));

            // Mark parent obsolete.
            this.CategoryManager.SetCategoryObsolete(parent.Id, true);

            // Parent is obsolete.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(description, type, parent.Id, iconPack, iconName, iconColor));
        }

        #endregion CreateCategory

        #region SetCategoryObsolete

        /// <summary>
        /// Tests the good flow of the SetCategoryObsolete method.
        /// </summary>
        [Fact]
        public void SetCategoryObsolete()
        {
            var parent = this.GenerateCategory();
            var child = this.GenerateCategory(parentCategoryId: parent.Id);
            var grandChild = this.GenerateCategory(parentCategoryId: child.Id);

            // Mark parent obsolete.
            this.CategoryManager.SetCategoryObsolete(parent.Id, true);

            // Retrieve updated category
            var updated = this.CategoryManager.GetCategory(parent.Id);

            // Verify category is no longer default and is obsolete.
            Assert.True(updated.IsObsolete);

            // Verify children are also obsolete.
            var updatedChild = this.CategoryManager.GetCategory(child.Id);
            var updatedGrandChild = this.CategoryManager.GetCategory(grandChild.Id);
            Assert.True(updatedChild.IsObsolete);
            Assert.True(updatedGrandChild.IsObsolete);

            // Mark parent active again.
            this.CategoryManager.SetCategoryObsolete(parent.Id, false);
            updated = this.CategoryManager.GetCategory(parent.Id);

            // Verify category is no longer obsolete.
            Assert.False(updated.IsObsolete);

            // Verify children are still obsolete.
            updatedChild = this.CategoryManager.GetCategory(child.Id);
            updatedGrandChild = this.CategoryManager.GetCategory(grandChild.Id);
            Assert.True(updatedChild.IsObsolete);
            Assert.True(updatedGrandChild.IsObsolete);
        }

        /// <summary>
        /// Tests the exceptional flow of the SetCategoryObsolete method.
        /// </summary>
        [Fact]
        public void SetCategoryObsolete_Exceptions()
        {
            // Create category.
            var category = this.GenerateCategory(description: "Description");
            var child = this.GenerateCategory(parentCategoryId: category.Id);

            // Mark category (and child) obsolete.
            this.CategoryManager.SetCategoryObsolete(category.Id, true);

            // Try to set child active.
            Assert.Throws<ValidationException>(() => this.CategoryManager.SetCategoryObsolete(child.Id, false));

            // Create category with same description as inactive category.
            var category2 = this.GenerateCategory(description: "Description");

            // Try to restore first category, now a category with same description exists.
            Assert.Throws<ValidationException>(() => this.CategoryManager.SetCategoryObsolete(category.Id, false));
        }

        #endregion SetCategoryObsolete
    }
}
