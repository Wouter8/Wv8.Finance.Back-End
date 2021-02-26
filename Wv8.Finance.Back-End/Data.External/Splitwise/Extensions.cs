namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Data.External.Splitwise.Models;
    using DT = PersonalFinance.Data.External.Splitwise.DataTransfer;

    /// <summary>
    /// A class containing extension methods related to the Splitwise integration.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts an expense from Splitwise to a domain object.
        /// </summary>
        /// <param name="expense">The expense from Splitwise.</param>
        /// <param name="userId">The user id.</param>
        /// <returns>The domain expense object.</returns>
        public static Expense ToDomainObject(this DT.Expense expense, int userId)
        {
            var user = expense.Users.Single(u => u.UserId == userId);

            return new Expense
            {
                Id = expense.Id,
                Date = LocalDate.FromDateTime(DateTime.Parse(expense.DateString)),
                Description = expense.Description,
                UpdatedAt = DateTime.Parse(expense.UpdatedAtString),
                IsDeleted = expense.DeletedAtString != null,
                PaidAmount = user.PaidShare,
                PersonalAmount = user.OwedShare,
            };
        }

        /// <summary>
        /// Converts a user from Splitwise to a domain object.
        /// </summary>
        /// <param name="user">The user from Splitwise.</param>
        /// <returns>The domain expense object.</returns>
        public static User ToDomainObject(this DT.User user)
        {
            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }
    }
}