namespace Business.UnitTest.Tests
{
    using Wv8.Core.Exceptions;
    using Xunit;

    /// <summary>
    /// A class containing tests for the budget manager.
    /// </summary>
    public class BudgetTests : BaseTest
    {
        #region GetBudget

        /// <summary>
        /// Tests the good flow of the GetBudget method.
        /// </summary>
        [Fact]
        public void GetBudget()
        {
            var budget = this.GenerateBudget();
            // TODO: Add transactions
            var retrievedBudget = this.BudgetManager.GetBudget(budget.Id);

            this.AssertEqual(budget, retrievedBudget);
        }

        /// <summary>
        /// Tests the exceptional flow of the GetBudget method.
        /// </summary>
        [Fact]
        public void GetBudget_Exceptions()
        {
            Assert.Throws<DoesNotExistException>(() =>
                this.BudgetManager.GetBudget(100));
        }

        #endregion

        #region GetBudgets

        [Fact]
        public void GetBudgets()
        {

        }

        #endregion
    }
}