namespace PersonalFinance.Data.History
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// An interface for an entity which is only valid for a period of time.
    /// </summary>
    public interface IHistoricalEntity : ICloneable
    {
        /// <summary>
        /// The date and time from which this entity is valid.
        /// </summary>
        [Key]
        DateTime ValidFrom { get; set; }

        /// <summary>
        /// The date and time till which this entity is valid.
        /// </summary>
        DateTime ValidTo { get; set; }
    }
}