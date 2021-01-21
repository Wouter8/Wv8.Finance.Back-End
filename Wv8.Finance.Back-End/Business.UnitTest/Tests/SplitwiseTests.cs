namespace Business.UnitTest.Tests
{
    using System;
    using Business.UnitTest.Helpers;
    using PersonalFinance.Business.Splitwise;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="SplitwiseManager"/>.
    /// </summary>
    public class SplitwiseTests : BaseTest
    {
        [Fact]
        public void Test1()
        {
            this.SplitwiseContextMock.GenerateExpense(1);
            Assert.Single(this.SplitwiseContextMock.GetExpenses(DateTime.MinValue));
        }

        [Fact]
        public void Test2()
        {
            this.SplitwiseContextMock.GenerateExpense(2);
            Assert.Single(this.SplitwiseContextMock.GetExpenses(DateTime.MinValue));
        }
    }
}