namespace Business.UnitTest.Tests
{
    using System;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Common.Enums;
    using Wv8.Core;
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A class containing tests for the budget manager.
    /// </summary>
    public class BudgetTests : BaseTest
    {
        #region GetBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.GetBudget"/> method.
        /// TODO: Create transactions and test spent is set correctly.
        /// </summary>
        [Fact]
        public void GetBudget()
        {
            var budget = this.GenerateBudget();
            var retrievedBudget = this.BudgetManager.GetBudget(budget.Id);

            this.AssertEqual(budget, retrievedBudget);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.GetBudget"/> method.
        /// </summary>
        [Fact]
        public void GetBudget_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.GetBudget(100));
        }

        #endregion GetBudget

        #region GetBudgets

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.GetBudgets"/> method.
        /// </summary>
        [Fact]
        public void GetBudgets()
        {
            var budget1 = this.GenerateBudget();
            var budget2 = this.GenerateBudget();
            var budget3 = this.GenerateBudget();

            var retrievedBudgets = this.BudgetManager.GetBudgets();

            Assert.Equal(3, retrievedBudgets.Count);

            this.CategoryManager.SetCategoryObsolete(budget1.CategoryId, true);
            retrievedBudgets = this.BudgetManager.GetBudgets();

            Assert.Equal(2, retrievedBudgets.Count);
        }

        #endregion GetBudgets

        #region GetBudgetsByFilter

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.GetBudgetsByFilter"/> method.
        /// </summary>
        [Fact]
        public void GetBudgetsByFilter()
        {
            var category1 = this.GenerateCategory();
            var category2 = this.GenerateCategory();

            // In the past so that GenerateBudget does not collide with this date
            var startDate = new DateTime(2020, 01, 01);
            var endDate = new DateTime(2020, 02, 01);

            var budget1 = this.GenerateBudget(category1.Id);
            var budget2 = this.GenerateBudget(category2.Id);
            var budget3 = this.GenerateBudget(categoryId: category1.Id, startDate: startDate, endDate: endDate);

            var retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(Maybe<int>.None, Maybe<string>.None, Maybe<string>.None);
            Assert.Equal(3, retrievedBudgets.Count);

            retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(category2.Id, Maybe<string>.None, Maybe<string>.None);
            Assert.Single(retrievedBudgets);

            retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(category1.Id, Maybe<string>.None, Maybe<string>.None);
            Assert.Equal(2, retrievedBudgets.Count);

            retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(
                    category1.Id,
                    new DateTime(2020, 01, 10).ToString("O"),
                    new DateTime(2020, 01, 15).ToString("O"));
            Assert.Single(retrievedBudgets);

            retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(
                    category1.Id,
                    new DateTime(2019, 12, 10).ToString("O"),
                    new DateTime(2020, 01, 15).ToString("O"));
            Assert.Single(retrievedBudgets);
        }

        #endregion GetBudgetsByFilter

        #region UpdateBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.UpdateBudget"/> method.
        /// TODO: Create transactions and test spent is set correctly.
        /// </summary>
        [Fact]
        public void UpdateBudget()
        {
            var budget = this.GenerateBudget();

            const string newDescription = "Description";
            const decimal newAmount = 5;
            var newStartDate = new DateTime(2019, 12, 01).ToString("O");
            var newEndDate = new DateTime(2019, 12, 30).ToString("O");

            var updated =
                this.BudgetManager.UpdateBudget(budget.Id, newDescription, newAmount, newStartDate, newEndDate);

            Assert.Equal(budget.Id, updated.Id);
            Assert.Equal(newDescription, updated.Description);
            Assert.Equal(newAmount, updated.Amount);
            Assert.Equal(newStartDate, updated.StartDate);
            Assert.Equal(newEndDate, updated.EndDate);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.UpdateBudget"/> method.
        /// </summary>
        [Fact]
        public void UpdateBudget_Exceptions()
        {
            var category = this.GenerateCategory();
            var category2 = this.GenerateCategory();

            var budget = this.GenerateBudget(category.Id, "Description");
            var budget2 = this.GenerateBudget(category2.Id, "Description2");

            // Description already exists.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.UpdateBudget(
                    budget2.Id,
                    "Description",
                    budget2.Amount,
                    budget2.StartDate,
                    budget2.EndDate));

            this.CategoryManager.SetCategoryObsolete(category.Id, true);
            this.BudgetManager.UpdateBudget(
                budget2.Id,
                "Description",
                budget2.Amount,
                budget2.StartDate,
                budget2.EndDate);

            this.CategoryManager.SetCategoryObsolete(category2.Id, true);
            // Category is obsolete.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.UpdateBudget(
                    budget2.Id,
                    "Description",
                    budget2.Amount,
                    budget2.StartDate,
                    budget2.EndDate));

            // Budget doesn't exist.
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.UpdateBudget(
                    100,
                    "Description",
                    100,
                    DateTime.Today.ToString("O"),
                    DateTime.Today.AddDays(1).ToString("O")));
        }

        #endregion UpdateBudget

        #region CreateBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.CreateBudget"/> method.
        /// TODO: Create transactions and test spent is set correctly.
        /// </summary>
        [Fact]
        public void CreateBudget()
        {
            var category = this.GenerateCategory();

            const string description = "Description";
            const decimal amount = 5;
            var startDate = new DateTime(2019, 12, 01).ToString("O");
            var endDate = new DateTime(2019, 12, 15).ToString("O");

            var budget = this.BudgetManager.CreateBudget(description, category.Id, amount, startDate, endDate);

            Assert.Equal(description, budget.Description);
            Assert.Equal(amount, budget.Amount);
            Assert.Equal(startDate, budget.StartDate);
            Assert.Equal(endDate, budget.EndDate);
            Assert.Equal(category.Id, budget.CategoryId);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.CreateBudget"/> method.
        /// </summary>
        [Fact]
        public void CreateBudget_Exceptions()
        {
            var category = this.GenerateCategory();
            var categoryIncome = this.GenerateCategory(CategoryType.Income);

            const string description = "Description";
            const decimal amount = 5;
            var startDate = new DateTime(2019, 12, 01).ToString("O");
            var endDate = new DateTime(2019, 12, 15).ToString("O");

            // Category does not exist.
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.CreateBudget(description, 100, amount, startDate, endDate));
            // Category is not expense type.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.CreateBudget(description, categoryIncome.Id, amount, startDate, endDate));

            this.CategoryManager.SetCategoryObsolete(category.Id, true);
            // Category is obsolete.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.CreateBudget(description, category.Id, amount, startDate, endDate));

            this.CategoryManager.SetCategoryObsolete(category.Id, false);
            this.BudgetManager.CreateBudget(description, category.Id, amount, startDate, endDate);

            // Category with same description already exists.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.CreateBudget(description, category.Id, amount, startDate, endDate));
        }

        #endregion CreateBudget

        #region DeleteBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.DeleteBudget"/> method.
        /// </summary>
        [Fact]
        public void DeleteBudget()
        {
            var budget = this.GenerateBudget();

            this.BudgetManager.DeleteBudget(budget.Id);

            Assert.Throws<DoesNotExistException>(() => this.BudgetManager.GetBudget(budget.Id));
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.DeleteBudget"/> method.
        /// </summary>
        [Fact]
        public void DeleteBudget_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() => this.BudgetManager.DeleteBudget(100));
        }

        #endregion DeleteBudget
    }
}