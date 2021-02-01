namespace Business.UnitTest.Tests
{
    using System.Linq;
    using Business.UnitTest.Helpers;
    using NodaTime;
    using PersonalFinance.Business.Splitwise;
    using Xunit;

    /// <summary>
    /// A test class testing the functionality of the <see cref="SplitwiseManager"/>.
    /// </summary>
    public class SplitwiseTests : BaseTest
    {
        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that an empty list is returned if the database is empty.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_Empty()
        {
            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
            Assert.Empty(transactions);
        }

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that the imported transactions are correctly filtered.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_ImportedFilter()
        {
            this.context.GenerateSplitwiseTransaction(1, imported: false);
            this.context.GenerateSplitwiseTransaction(2, imported: true);
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);
            Assert.Equal(2, transactions.Count);

            transactions = this.SplitwiseManager.GetSplitwiseTransactions(false);
            Assert.Single(transactions);
            Assert.Equal(1, transactions.Single().Id);
        }

        /// <summary>
        /// Test the <see cref="SplitwiseManager.GetSplitwiseTransactions"/> method.
        /// Verifies that the returned transactions are correctly ordered on date.
        /// </summary>
        [Fact]
        public void Test_GetSplitwiseTransactions_Ordering()
        {
            this.context.GenerateSplitwiseTransaction(1, date: new LocalDate(2021, 1, 5));
            this.context.GenerateSplitwiseTransaction(2, date: new LocalDate(2021, 1, 4));
            this.context.GenerateSplitwiseTransaction(3, date: new LocalDate(2021, 1, 6));
            this.context.SaveChanges();

            var transactions = this.SplitwiseManager.GetSplitwiseTransactions(true);

            Assert.Equal(2, transactions.First().Id);
            Assert.Equal(1, transactions.Skip(1).First().Id);
            Assert.Equal(3, transactions.Skip(2).First().Id);
        }
    }
}