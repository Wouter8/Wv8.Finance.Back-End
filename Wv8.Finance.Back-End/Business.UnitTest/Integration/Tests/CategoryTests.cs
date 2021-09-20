namespace Business.UnitTest.Integration.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Business.UnitTest.Integration.Helpers;
    using PersonalFinance.Common.DataTransfer.Output;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A class containing tests for the category manager.
    /// </summary>
    public class CategoryTests : BaseIntegrationTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryTests"/> class.
        /// </summary>
        /// <param name="spFixture">See <see cref="BaseIntegrationTest"/>.</param>
        public CategoryTests(ServiceProviderFixture spFixture)
            : base(spFixture)
        {
        }


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
        public void GetCategories_Group()
        {
            // Empty database.
            var retrievedCategories = this.CategoryManager.GetCategories(true, true);
            Assert.Empty(retrievedCategories);

            // Create categories.
            const int categoryCount = 5;
            const int childrenCount = 3;
            var parentCategories = new List<Category>();
            for (int i = 0; i < categoryCount; i++)
            {
                var parent = this.GenerateCategory();
                for (int j = 0; j < childrenCount; j++)
                {
                    var child = this.GenerateCategory(parentCategoryId: parent.Id);
                    parent.Children.Add(child);
                }
                parentCategories.Add(parent);
            }

            // Load all active categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(false, true);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Verify categories.
            foreach (var savedCategory in parentCategories)
            {
                var retrievedCategory = retrievedCategories.Single(a => a.Id == savedCategory.Id);
                Assert.Equal(retrievedCategory.Children.Count, childrenCount);
            }

            // Load active and inactive categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(true, true);
            Assert.Equal(categoryCount, retrievedCategories.Count);

            // Set category obsolete
            this.CategoryManager.SetCategoryObsolete(parentCategories.Last().Id, true);

            // Load all active categories (all except 1)
            retrievedCategories = this.CategoryManager.GetCategories(false, true);
            Assert.Equal(categoryCount - 1, retrievedCategories.Count);
            Assert.True(retrievedCategories.All(c => c.Id != parentCategories.Last().Id));

            // Load active and inactive categories (should return all again).
            retrievedCategories = this.CategoryManager.GetCategories(true, true);
            Assert.Equal(categoryCount, retrievedCategories.Count);
        }

        /// <summary>
        /// Tests the good flow of the GetCategories method.
        /// </summary>
        [Fact]
        public void GetCategories_NoGroup()
        {
            // Empty database.
            var retrievedCategories = this.CategoryManager.GetCategories(true, false);
            Assert.Empty(retrievedCategories);

            // Create categories.
            const int categoryCount = 5;
            const int childrenCount = 3;
            const int totalCategoryCount = 20;
            var parentCategories = new List<Category>();
            var childCategories = new List<Category>();
            for (int i = 0; i < categoryCount; i++)
            {
                var parent = this.GenerateCategory();
                for (int j = 0; j < childrenCount; j++)
                {
                    var child = this.GenerateCategory(parentCategoryId: parent.Id);
                    childCategories.Add(child);
                    parent.Children.Add(child);
                }
                parentCategories.Add(parent);
            }

            // Load all active categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(false, false);
            Assert.Equal(totalCategoryCount, retrievedCategories.Count);

            // Verify categories.
            foreach (var savedCategory in parentCategories)
            {
                var retrievedCategory = retrievedCategories.Single(a => a.Id == savedCategory.Id);
                Assert.Equal(retrievedCategory.Children.Count, childrenCount);
            }
            foreach (var savedCategory in childCategories)
            {
                var retrievedCategory = retrievedCategories.Single(a => a.Id == savedCategory.Id);
                Assert.True(retrievedCategory.ParentCategory.IsSome);
            }

            // Load active and inactive categories (all active).
            retrievedCategories = this.CategoryManager.GetCategories(true, false);
            Assert.Equal(totalCategoryCount, retrievedCategories.Count);

            // Set category obsolete
            this.CategoryManager.SetCategoryObsolete(parentCategories.Last().Id, true);

            // Load all active categories (all except 4)
            retrievedCategories = this.CategoryManager.GetCategories(false, false);
            Assert.Equal(totalCategoryCount - 4, retrievedCategories.Count);
            Assert.True(retrievedCategories.All(c => c.Id != parentCategories.Last().Id));

            // Load active and inactive categories (should return all again).
            retrievedCategories = this.CategoryManager.GetCategories(true, false);
            Assert.Equal(totalCategoryCount, retrievedCategories.Count);
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
            var category3 = this.GenerateCategory();
            var child = this.GenerateCategory(parentCategoryId: category1.Id);

            this.CategoryManager.SetCategoryObsolete(category2.Id, true);

            var retrievedCategories = this.CategoryManager.GetCategoriesByFilter(true, true);
            Assert.Equal(3, retrievedCategories.Count);
            retrievedCategories = this.CategoryManager.GetCategoriesByFilter(true, false);
            Assert.Equal(4, retrievedCategories.Count);
            retrievedCategories = this.CategoryManager.GetCategoriesByFilter(false, true);
            Assert.Equal(2, retrievedCategories.Count);
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
            const decimal newExpectedMonthlyAmount = -50;
            const string newIconPack = "fas";
            const string newIconName = "circle";
            const string newIconColor = "#FFFFFF";

            var updated = this.CategoryManager.UpdateCategory(
                category.Id, newDescription, newExpectedMonthlyAmount, parent.Id, newIconPack, newIconName, newIconColor);

            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newExpectedMonthlyAmount, updated.ExpectedMonthlyAmount.Value);
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
            const string diffDescription = "Description2";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category.Id, newDescription, Maybe<decimal>.None, Maybe<int>.None, newIconPack, newIconName, newIconColor));

            // Description already exists, but on parent.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category.Id, newDescription, Maybe<decimal>.None, category2.Id, newIconPack, newIconName, newIconColor));

            // Expected monthly amount exceeds parents' expected monthly amount.
            this.CategoryManager.UpdateCategory(
                category2.Id, category2.Description, -5, Maybe<int>.None, category2.Icon.Pack, category2.Icon.Name, category2.Icon.Color);
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category.Id, diffDescription, -10, category2.Id, newIconPack, newIconName, newIconColor));

            // Expected monthly amount of all children exceeds parents' expected monthly amount.
            this.CategoryManager.CreateCategory(
                "Description3", -4, category2.Id, newIconPack, newIconName, newIconColor);
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category.Id, diffDescription, -2, category2.Id, newIconPack, newIconName, newIconColor));

            // Expected monthly amount of children exceeds expected monthly amount.
            this.CategoryManager.UpdateCategory(
                category.Id, category.Description, Maybe<decimal>.None, category2.Id, category.Icon.Pack, category.Icon.Name, category.Icon.Color);
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category2.Id, category2.Description, -3, Maybe<int>.None, category2.Icon.Pack, category2.Icon.Name, category2.Icon.Color));

            // Category with same description is obsolete.
            this.CategoryManager.SetCategoryObsolete(category2.Id, true);
            this.CategoryManager.UpdateCategory(
                category.Id, newDescription, Maybe<decimal>.None, Maybe<int>.None, newIconPack, newIconName, newIconColor);

            // Parent category is obsolete.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.UpdateCategory(
                    category.Id, diffDescription, Maybe<decimal>.None, category2.Id, newIconPack, newIconName, newIconColor));

            // Category doesn't exist.
            Assert.Throws<DoesNotExistException>(
                () => this.CategoryManager.UpdateCategory(
                    100, diffDescription, Maybe<decimal>.None, Maybe<int>.None, newIconPack, newIconName, newIconColor));
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
            const decimal expectedMonthlyAmount = -50;
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            var parent = this.CategoryManager.CreateCategory(
                description, expectedMonthlyAmount, Maybe<int>.None, iconPack, iconName, iconColor);

            Assert.Equal(description, parent.Description);
            Assert.Equal(expectedMonthlyAmount, parent.ExpectedMonthlyAmount.Value);
            Assert.Equal(iconPack, parent.Icon.Pack);
            Assert.Equal(iconName, parent.Icon.Name);
            Assert.Equal(iconColor, parent.Icon.Color);
            Assert.False(parent.IsObsolete);
            Assert.False(parent.ParentCategoryId.IsSome);
            Assert.False(parent.ParentCategory.IsSome);

            // Create new category with first category as parent.
            const string description2 = "Description2";
            var child = this.CategoryManager.CreateCategory(
                description2, expectedMonthlyAmount, parent.Id, iconPack, iconName, iconColor);

            Assert.True(child.ParentCategoryId.IsSome);
            Assert.True(child.ParentCategory.IsSome);
            Assert.Equal(expectedMonthlyAmount, child.ExpectedMonthlyAmount.Value);

            Assert.Equal(parent.Id, child.ParentCategoryId.Value);
        }

        /// <summary>
        /// Tests the exceptional flow of the CreateCategory method.
        /// </summary>
        [Fact]
        public void CreateCategory_Exceptions()
        {
            var parent = this.GenerateCategory(description: "Description");

            const string description = "Description2";
            const string iconPack = "fas";
            const string iconName = "circle";
            const string iconColor = "#FFFFFF";

            // Description already exists.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(
                    "Description", Maybe<decimal>.None, Maybe<int>.None, iconPack, iconName, iconColor));

            // Expected monthly amount exceeds parents' expected monthly amount.
            this.CategoryManager.UpdateCategory(
                parent.Id, parent.Description, -5, Maybe<int>.None, parent.Icon.Pack, parent.Icon.Name, parent.Icon.Color);
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(
                    description, -10, parent.Id, iconPack, iconName, iconColor));

            // Expected monthly amount of all children exceeds parents' expected monthly amount.
            this.CategoryManager.CreateCategory(
                "Description3", -4, parent.Id, iconPack, iconName, iconColor);
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(
                    "Description3", -10, parent.Id, iconPack, iconName, iconColor));

            // Mark parent obsolete.
            this.CategoryManager.SetCategoryObsolete(parent.Id, true);

            // Parent is obsolete.
            Assert.Throws<ValidationException>(
                () => this.CategoryManager.CreateCategory(
                    description, Maybe<decimal>.None, parent.Id, iconPack, iconName, iconColor));
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
