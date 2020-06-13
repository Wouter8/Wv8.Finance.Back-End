﻿namespace PersonalFinance.Data.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// An entity representing an account. Used for different bank accounts, etc.
    /// </summary>
    public class AccountEntity
    {
        /// <summary>
        /// The identifier of this account.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The description of this account.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A value indicating if this account is the default account for new transactions.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// A value indicating if this account is obsolete. No new transactions can be created for this account.
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// The identifier of the icon for this account.
        /// </summary>
        public int IconId { get; set; }

        /// <summary>
        /// The icon for this account.
        /// </summary>
        public IconEntity Icon { get; set; }

        /// <summary>
        /// The historical entities for this account.
        /// </summary>
        public List<AccountHistoryEntity> History { get; set; }

        /// <summary>
        /// The historical balances for this account.
        /// </summary>
        public List<DailyBalanceEntity> DailyBalances { get; set; }
    }
}