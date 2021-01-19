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
            var others = expense.Users.Where(u => u.UserId != userId);

            Expense domainObject;

            if (user.PaidShare > 0)
            {
                domainObject = new PaidExpense
                {
                    PaidAmount = user.PaidShare,
                    PersonalAmount = user.OwedShare,
                    // TODO: Can the payment be split? If so, the paid share should be subtracted from the owed share.
                    OwedAmount = others.Sum(u => u.OwedShare),
                };
            }
            else
            {
                domainObject = new OwedExpense
                {
                    AmountOwed = user.OwedShare,
                };
            }

            domainObject.Id = expense.Id;
            domainObject.Date = LocalDate.FromDateTime(DateTime.Parse(expense.DateString));
            domainObject.Description = expense.Description;
            domainObject.IsDeleted = expense.DeletedAtString != null;

            return domainObject;
        }
    }
}