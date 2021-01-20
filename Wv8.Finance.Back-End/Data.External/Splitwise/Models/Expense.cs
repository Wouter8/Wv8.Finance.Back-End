namespace PersonalFinance.Data.External.Splitwise.Models
{
    using System;
    using NodaTime;
    using Wv8.Core;

    /// <summary>
    /// A transaction from Splitwise.
    /// </summary>
    public class Expense
    {
        /// <summary>
        /// The identifier of the expense.
        /// </summary>
        public int Id { get; set; }

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
        /// The amount paid by the user.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// The personal share of the expense.
        /// </summary>
        public decimal PersonalAmount { get; set; }

        /// <summary>
        /// The amount that is owed by others.
        /// This is equal to <see cref="PaidAmount"/> minus <see cref="PersonalAmount"/> and can never be less than 0.
        /// </summary>
        public decimal OwedByOthers => Math.Max(0, this.PaidAmount - this.PersonalAmount);

        /// <summary>
        /// The part of the expense which is owed to others.
        /// This is equal to <see cref="PersonalAmount"/> minus <see cref="PaidAmount"/> and can never be less than 0.
        /// </summary>
        public decimal OwedToOthers => Math.Max(0, this.PersonalAmount - this.PaidAmount);
    }
}