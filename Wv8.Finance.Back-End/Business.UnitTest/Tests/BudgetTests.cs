namespace Business.UnitTest.Tests
{
    using System;
    using NodaTime;
    using PersonalFinance.Business.Budget;
    using PersonalFinance.Common;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Exceptions;
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
        /// </summary>
        [Fact]
        public void GetBudget()
        {
            var category = this.GenerateCategory();
            var transaction = this.GenerateTransaction(categoryId: category.Id, amount: -50);
            var budget = this.GenerateBudget(category.Id);
            var retrievedBudget = this.BudgetManager.GetBudget(budget.Id);

            this.AssertEqual(budget, retrievedBudget);
            Assert.Equal(50, retrievedBudget.Spent);
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
            var startDate = new LocalDate(2020, 01, 01);
            var endDate = new LocalDate(2020, 02, 01);

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
                    new LocalDate(2020, 01, 10).ToString(),
                    new LocalDate(2020, 01, 15).ToString());
            Assert.Single(retrievedBudgets);

            retrievedBudgets =
                this.BudgetManager.GetBudgetsByFilter(
                    category1.Id,
                    new LocalDate(2019, 12, 10).ToString(),
                    new LocalDate(2020, 01, 15).ToString());
            Assert.Single(retrievedBudgets);
        }

        #endregion GetBudgetsByFilter

        #region UpdateBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.UpdateBudget"/> method.
        /// </summary>
        [Fact]
        public void UpdateBudget()
        {
            var category = this.GenerateCategory();
            var transaction1 =
                this.GenerateTransaction(categoryId: category.Id, amount: -30, date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-2));
            var transaction2 =
                this.GenerateTransaction(categoryId: category.Id, amount: -30, date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1));
            var budget = this.GenerateBudget(category.Id);

            const decimal newAmount = 5;
            var newStartDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-3).ToDateString();
            var newEndDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-2).ToDateString();

            var updated =
                this.BudgetManager.UpdateBudget(budget.Id, newAmount, newStartDate, newEndDate);

            Assert.Equal(budget.Id, updated.Id);
            Assert.Equal(newAmount, updated.Amount);
            Assert.Equal(newStartDate, updated.StartDate);
            Assert.Equal(newEndDate, updated.EndDate);
            Assert.Equal(30, updated.Spent);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.UpdateBudget"/> method.
        /// </summary>
        [Fact]
        public void UpdateBudget_Exceptions()
        {
            var category = this.GenerateCategory();
            var category2 = this.GenerateCategory();

            var budget = this.GenerateBudget(category.Id);
            var budget2 = this.GenerateBudget(category2.Id);

            this.CategoryManager.SetCategoryObsolete(category.Id, true);
            this.BudgetManager.UpdateBudget(
                budget2.Id,
                budget2.Amount,
                budget2.StartDate,
                budget2.EndDate);

            this.CategoryManager.SetCategoryObsolete(category2.Id, true);
            // Category is obsolete.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.UpdateBudget(
                    budget2.Id,
                    budget2.Amount,
                    budget2.StartDate,
                    budget2.EndDate));

            // Budget doesn't exist.
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.UpdateBudget(
                    100,
                    100,
                    LocalDate.FromDateTime(DateTime.Today).ToString(),
                    LocalDate.FromDateTime(DateTime.Today).PlusDays(1).ToString()));
        }

        #endregion UpdateBudget

        #region CreateBudget

        /// <summary>
        /// Tests the good flow of the <see cref="IBudgetManager.CreateBudget"/> method.
        /// </summary>
        [Fact]
        public void CreateBudget()
        {
            var category = this.GenerateCategory();
            var transaction = this.GenerateTransaction(categoryId: category.Id, amount: -30, date: LocalDate.FromDateTime(DateTime.Today).PlusDays(-1));

            const decimal amount = 5;
            var startDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-2).ToDateString();
            var endDate = LocalDate.FromDateTime(DateTime.Today).PlusDays(-1).ToDateString();

            var budget = this.BudgetManager.CreateBudget(category.Id, amount, startDate, endDate);

            Assert.Equal(amount, budget.Amount);
            Assert.Equal(startDate, budget.StartDate);
            Assert.Equal(endDate, budget.EndDate);
            Assert.Equal(category.Id, budget.CategoryId);
            Assert.Equal(30, budget.Spent);
        }

        /// <summary>
        /// Tests the exceptional flow of the <see cref="IBudgetManager.CreateBudget"/> method.
        /// </summary>
        [Fact]
        public void CreateBudget_Exceptions()
        {
            var category = this.GenerateCategory();
            var categoryIncome = this.GenerateCategory(CategoryType.Income);

            const decimal amount = 5;
            var startDate = new LocalDate(2019, 12, 01).ToString();
            var endDate = new LocalDate(2019, 12, 15).ToString();

            // Category does not exist.
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.CreateBudget(100, amount, startDate, endDate));
            // Category is not expense type.
            Assert.Throws<ValidationException>(() =>
                this.BudgetManager.CreateBudget(categoryIncome.Id, amount, startDate, endDate));

            this.CategoryManager.SetCategoryObsolete(category.Id, true);
            // Category is obsolete.
            Assert.Throws<IsObsoleteException>(() =>
                this.BudgetManager.CreateBudget(category.Id, amount, startDate, endDate));
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