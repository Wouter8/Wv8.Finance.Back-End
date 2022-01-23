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
        Days = 1,

        /// <summary>
        /// Happens every x weeks.
        /// </summary>
        Weeks = 2,

        /// <summary>
        /// Happens every x months.
        /// </summary>
        Months = 3,

        /// <summary>
        /// Happens every x years.
        /// </summary>
        Years = 4,
    }
}