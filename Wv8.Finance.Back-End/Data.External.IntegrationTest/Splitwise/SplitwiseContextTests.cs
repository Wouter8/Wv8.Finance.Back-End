namespace Data.External.IntegrationTest.Splitwise
{
    using System;
    using PersonalFinance.Data.External.Splitwise;
    using Xunit;

    /// <summary>
    /// A test class for the <see cref="SplitwiseContext"/>.
    /// </summary>
    /// <remarks>These tests should be ran manually if any logic for communicating with Splitwise is changed.</remarks>
    public class SplitwiseContextTests : BaseTest
    {
        [Fact]
        public void Test()
        {
            var expenses = this.splitwiseContext.GetExpenses(DateTime.MinValue);
        }
    }
}