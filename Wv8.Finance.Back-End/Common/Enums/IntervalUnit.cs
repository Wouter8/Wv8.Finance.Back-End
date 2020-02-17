namespace PersonalFinance.Common.Enums
{
    /// <summary>
    /// An enum representing the units for a recurring entity.
    /// </summary>
    public enum IntervalUnit
    {
        /// <summary>
        /// Happens every x days.
        /// </summary>
        Day = 1,

        /// <summary>
        /// Happens every x weeks.
        /// </summary>
        Week = 2,

        /// <summary>
        /// Happens every x months.
        /// </summary>
        Month = 3,

        /// <summary>
        /// Happens every x years.
        /// </summary>
        Year = 4,
    }
}