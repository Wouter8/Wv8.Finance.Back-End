namespace PersonalFinance.Data.Models
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// A transaction which is imported from Splitwise.
    /// A category is to be added to be able to fully import this transaction into the finance application.
    /// </summary>
    public class SplitwiseTransactionEntity
    {
        /// <summary>
        /// The identifier of the transaction.
        /// </summary>
        /// <remarks>This identifier is directly imported from Splitwise.</remarks>
        public long Id { get; set; }

        /// <summary>
        /// The description of the transaction.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The date of the transaction.
        /// </summary>
        public LocalDate Date { get; set; }

        /// <summary>
        /// <c>true</c> if the transaction has been deleted, <c>false</c> otherwise.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The amount paid by the user.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// The personal share of the transaction.
        /// </summary>
        public decimal PersonalAmount { get; set; }

        /// <summary>
        /// <c>true</c> if the transaction has be imported completely, that is a <see cref="TransactionEntity"/> has
        /// been created for this transaction, <c>false</c> otherwise.
        /// </summary>
        public bool Imported { get; set; }

        /// <summary>
        /// The timestamp at which this transaction was last modified in Splitwise.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// The splits, containing all specifications of amounts in this transaction paid for others.
        /// </summary>
        public List<SplitDetailEntity> SplitDetails { get; set; }

        /// <summary>
        /// A value indicating if the user had anything to do with the transaction. <c>true</c> if the user either paid
        /// or had a personal amount.
        /// </summary>
        public bool HasShare => this.PersonalAmount != 0 || this.PaidAmount != 0;

        /// <summary>
        /// <c>true</c> if the transaction is importable, <c>false</c> otherwise.
        /// </summary>
        public bool Importable => !this.IsDeleted && !this.Imported && this.HasShare;

        /// <summary>
        /// The amount that is owed by others.
        /// This is equal to <see cref="PaidAmount"/> minus <see cref="PersonalAmount"/> and can never be less than 0.
        /// </summary>
        public decimal OwedByOthers => Math.Max(0, this.PaidAmount - this.PersonalAmount);

        /// <summary>
        /// The part of the transaction which is owed to others.
        /// This is equal to <see cref="PersonalAmount"/> minus <see cref="PaidAmount"/> and can never be less than 0.
        /// </summary>
        public decimal OwedToOthers => Math.Max(0, this.PersonalAmount - this.PaidAmount);
    }
}
