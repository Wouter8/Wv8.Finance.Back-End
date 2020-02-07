namespace Business.UnitTest.Tests
{
    using System.Linq;
    using System.Collections.Generic;
    using PersonalFinance.Common.DataTransfer;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    public class CategoryTests : BaseTest
    {

        #region GetCategory

        [Fact]
        public void GetCategory()
        {
            var savedCategory = this.GenerateCategory();
            var retrievedCategory = this.CategoryManager.GetCategory(savedCategory.Id);

            this.AssertEqual(savedCategory, retrievedCategory);
        }

        [Fact]
        public void GetCategory_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.CategoryManager.GetCategory(-1));
        }

        #endregion GetCategory

        #region GetCategories

        [Fact]
        public void GetCategories()
        {
            // Empty database.
            var retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Empty(retrievedCategories);

            // Create categories.
            const int categoryCount = 5;
            var savedCategories = new List<Category>();
            for (int i = 0; i < categoryCount; i++)
            {
                savedCategories.Add(this.GenerateCategory());
            }

            // Load all active categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(false);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Verify categories.
            foreach (var savedCategory in savedCategories)
            {
                var retrievedCategory = retrievedCategories.Single(a => a.Id == savedCategory.Id);

                this.AssertEqual(savedCategory, retrievedCategory);
            }

            // Load active and inactive categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Set category obsolete
            this.CategoryManager.SetCategoryObsolete(savedCategories.Last().Id, true);

            // Load all active categories (all except 1)
            retrievedCategories = this.CategoryManager.GetCategories(false);
            Assert.Equal(categoryCount - 1, retrievedCategories.Count);
            Assert.True(retrievedCategories.All(c => c.Id != savedCategories.Last().Id));

            // Load active and inactive categories (should return all again).
            retrievedCategories = this.CategoryManager.GetCategories(true);
            Assert.Equal(categoryCount, retrievedCategories.Count);
        }

        #endregion GetCategories

        #region UpdateCategory

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

        [Fact]
        public void SetCategoryObsolete_Exceptions()
        {
            // Create category.
            var category = this.GenerateCategory(description: "Description");
            
            // Mark category obsolete.
            this.CategoryManager.SetCategoryObsolete(category.Id, true);

            // Create category with same description as inactive category.
            var category2 = this.GenerateCategory(description: "Description");

            // Try to restore first category, now a category with same description exists.
            Assert.Throws<ValidationException>(() => this.CategoryManager.SetCategoryObsolete(category.Id, false));
        }

        #endregion SetCategoryObsolete
    }
}
