namespace PersonalFinance.Data.External.Splitwise.Models
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// A transaction from Splitwise.
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// The identifier of the expense.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The description of the expense.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date of the expense.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// <c>true</c> if the expense has been deleted, <c>false</c> otherwise.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The timestamp at which this transaction was last modified in Splitwise.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The amount paid by the user.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// The personal share of the expense.
        /// </summary>
        public decimal PersonalAmount { get; set; }

        /// <summary>
        /// The collection of splits. Only contains values if the user paid for this expense.
        /// </summary>
        public List<Split> Splits { get; set; }

        /// <summary>
        /// A value indicating if the user had anything to do with the expense. <c>true</c> if the user either paid or
        /// had a personal amount.
        /// </summary>
        public bool HasShare => this.PersonalAmount != 0 || this.PaidAmount != 0;
    }
}
