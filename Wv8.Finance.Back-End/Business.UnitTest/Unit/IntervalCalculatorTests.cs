namespace Business.UnitTest.Unit
{
    using System;
    using System.Linq;
    using Business.UnitTest.Integration.Helpers;
    using NodaTime;
    using PersonalFinance.Business;
    using PersonalFinance.Business.Shared;
    using PersonalFinance.Common.Enums;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="IntervalCalculator"/>.
    /// </summary>
    public class IntervalCalculatorTests
    {
        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of days are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Exact_Days()
        {
            // 12 days
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 12);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 12);

            Assert.Equal(ReportIntervalUnit.Days, unit);
            Assert.Equal(12, intervals.Count);

            for (var i = 0; i < 12; i++)
            {
                var expected = start.PlusDays(i);
                var interval = intervals[i];

                Assert.Equal(expected, interval.Start);
                Assert.Equal(expected, interval.End);
            }
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of days are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Less_Days()
        {
            // 2 days
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 2);

            // Max 12 intervals
            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 12);

            Assert.Equal(ReportIntervalUnit.Days, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start, end1);
            Assert.Equal(end, start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of weeks are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Exact_Weeks()
        {
            // 2 weeks
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 14);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 2);

            Assert.Equal(ReportIntervalUnit.Weeks, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusDays(6), end1);
            Assert.Equal(start.PlusDays(7), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of weeks are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Less_Weeks()
        {
            // 2 weeks
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 14);

            // Max 12 intervals
            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 12);

            Assert.Equal(ReportIntervalUnit.Weeks, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusDays(6), end1);
            Assert.Equal(start.PlusDays(7), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of months are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Exact_Months()
        {
            // 2 months
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 02, 28);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 2);

            Assert.Equal(ReportIntervalUnit.Months, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusMonths(1).PlusDays(-1), end1);
            Assert.Equal(start.PlusMonths(1), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of months are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Less_Months()
        {
            // 2 months
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 02, 28);

            // Max 3 intervals
            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 3);

            Assert.Equal(ReportIntervalUnit.Months, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusMonths(1).PlusDays(-1), end1);
            Assert.Equal(start.PlusMonths(1), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of years are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Exact_Years()
        {
            // 2 years
            var start = Ld(2021, 01, 01);
            var end = Ld(2022, 12, 31);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 2);

            Assert.Equal(ReportIntervalUnit.Years, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusYears(1).PlusDays(-1), end1);
            Assert.Equal(start.PlusYears(1), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that intervals of years are returned correctly.
        /// </summary>
        [Fact]
        public void GetIntervals_Less_Years()
        {
            // 2 years
            var start = Ld(2021, 01, 01);
            var end = Ld(2022, 12, 31);

            // Max 12 intervals
            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 12);

            Assert.Equal(ReportIntervalUnit.Years, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusYears(1).PlusDays(-1), end1);
            Assert.Equal(start.PlusYears(1), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that the last interval is capped at the end date.
        /// </summary>
        [Fact]
        public void GetIntervals_CappedEnd()
        {
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 11); // Less than 2 exact weeks

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end, 2);

            Assert.Equal(ReportIntervalUnit.Weeks, unit);
            Assert.Equal(2, intervals.Count);

            var (start1, end1) = intervals[0];
            var (start2, end2) = intervals[1];

            Assert.Equal(start, start1);
            Assert.Equal(start.PlusDays(6), end1);
            Assert.Equal(start.PlusDays(7), start2);
            Assert.Equal(end, end2);
        }

        /// <summary>
        /// Tests method <see cref="IntervalCalculator.GetIntervals(NodaTime.LocalDate,NodaTime.LocalDate,int)"/>.
        /// Verifies that the last interval is capped at the end date.
        /// </summary>
        [Fact]
        public void GetIntervals_TooBig()
        {
            // More than 2 years
            var start = Ld(2021, 01, 01);
            var end = Ld(2024, 01, 01);

            // Max 2 intervals
            Wv8Assert.Throws<InvalidOperationException>(
                () => IntervalCalculator.GetIntervals(start, end, 2),
                $"Not able to create a maximum of 2 intervals between 01-01-2021 and 01-01-2024.");
        }

        [Fact]
        public void GetIntervals_PreDefined_Day()
        {
            // 1 month
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 01, 31);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Days, unit);

            for (var i = 0; i < 31; i++)
            {
                Assert.Equal(start.PlusDays(i), intervals[i].Start);
                Assert.Equal(start.PlusDays(i), intervals[i].End);
            }
        }

        [Fact]
        public void GetIntervals_PreDefined_Day_MidMonth()
        {
            // 1 month
            var start = Ld(2021, 01, 15);
            var end = Ld(2021, 02, 14);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Days, unit);

            for (var i = 0; i < 31; i++)
            {
                Assert.Equal(start.PlusDays(i), intervals[i].Start);
                Assert.Equal(start.PlusDays(i), intervals[i].End);
            }

            // 6 days
            start = Ld(2021, 01, 15);
            end = Ld(2021, 01, 21);

            (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Days, unit);

            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(start.PlusDays(i), intervals[i].Start);
                Assert.Equal(start.PlusDays(i), intervals[i].End);
            }
        }

        [Fact]
        public void GetIntervals_PreDefined_Week()
        {
            // 1 month and 1 day
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 02, 01);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Weeks, unit);

            for (var i = 0; i < 4; i++)
            {
                Assert.Equal(start.PlusWeeks(i), intervals[i].Start);
                Assert.Equal(start.PlusWeeks(i).PlusDays(6), intervals[i].End);
            }
            Assert.Equal(start.PlusWeeks(4), intervals.Last().Start);
            Assert.Equal(end, intervals.Last().End);
        }

        [Fact]
        public void GetIntervals_PreDefined_Month()
        {
            // 6 months
            var start = Ld(2021, 01, 01);
            var end = Ld(2021, 06, 30);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Months, unit);

            for (var i = 0; i < 6; i++)
            {
                Assert.Equal(start.PlusMonths(i), intervals[i].Start);
                Assert.Equal(start.PlusMonths(i + 1).PlusDays(-1), intervals[i].End);
            }

            // 3 years
            start = Ld(2021, 01, 01);
            end = Ld(2023, 12, 31);

            (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Months, unit);

            for (var i = 0; i < 36; i++)
            {
                Assert.Equal(start.PlusMonths(i), intervals[i].Start);
                Assert.Equal(start.PlusMonths(i + 1).PlusDays(-1), intervals[i].End);
            }
        }

        [Fact]
        public void GetIntervals_PreDefined_Year()
        {
            // 3 years and 1 day
            var start = Ld(2021, 01, 01);
            var end = Ld(2024, 01, 01);

            var (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Years, unit);

            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(start.PlusYears(i), intervals[i].Start);
                Assert.Equal(start.PlusYears(i + 1).PlusDays(-1), intervals[i].End);
            }
            Assert.Equal(start.PlusYears(3), intervals.Last().Start);
            Assert.Equal(end, intervals.Last().End);

            // 1000 years
            start = Ld(2021, 01, 01);
            end = Ld(3021, 12, 31);

            (unit, intervals) = IntervalCalculator.GetIntervals(start, end);

            Assert.Equal(ReportIntervalUnit.Years, unit);

            for (var i = 0; i < 1000; i++)
            {
                Assert.Equal(start.PlusYears(i), intervals[i].Start);
                Assert.Equal(start.PlusYears(i + 1).PlusDays(-1), intervals[i].End);
            }
        }

        private static LocalDate Ld(int year, int month, int day)
        {
            return new LocalDate(year, month, day);
        }
    }
}
