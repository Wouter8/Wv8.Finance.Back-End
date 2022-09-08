namespace PersonalFinance.Business.Transaction
{
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using PersonalFinance.Common.DataTransfer.Reports;
    using PersonalFinance.Common.Enums;
    using PersonalFinance.Common.Extensions;
    using PersonalFinance.Data.Models;
    using Wv8.Core;
    using Wv8.Core.Collections;
    using Wv8.Core.EntityFramework;

    /// <summary>
    /// A class containing extension methods related to transactions.
    /// </summary>
    public static class TransactionExtensions
    {
        public static TransactionSums Sum(this List<TransactionEntity> transactions)
        {
            var expense = 0M;
            var income = 0M;
            foreach (var transaction in transactions)
            {
                if (transaction.Type == TransactionType.Expense)
                    expense -= transaction.PersonalAmount;
                else if (transaction.Type == TransactionType.Income)
                    income += transaction.PersonalAmount;
            }

            return new TransactionSums
            {
                Expense = expense,
                Income = income,
            };
        }

        public static Dictionary<DateInterval, List<TransactionEntity>> GroupByInterval(this List<TransactionEntity> transactions, List<DateInterval> intervals)
        {
            intervals = intervals.OrderBy(i => i.Start).ToList();

            var transactionsByInterval = new Dictionary<DateInterval, List<TransactionEntity>>();

            foreach (var transaction in transactions)
            {
                var interval = intervals.First(i => i.Start >= transaction.Date);

                var intervalTransactions = transactionsByInterval.TryGetList(interval);
                intervalTransactions.Add(transaction);

                transactionsByInterval[interval] = intervalTransactions;
            }

            return transactionsByInterval;
        }

        public static Dictionary<int, List<TransactionEntity>> GroupByCategory(this List<TransactionEntity> transactions)
        {
            // Transfer transactions do not have a category so are irrelevant here.
            transactions = transactions.Where(t => t.Type != TransactionType.Transfer).ToList();

            var categories = transactions
                .Select(t => t.Category)
                .Union(transactions.Where(t => t.Category.ParentCategoryId.HasValue).Select(t => t.Category.ParentCategory))
                .Distinct(c => c.Id)
                .ToList();
            var childCategories = new Dictionary<int, List<int>>();
            foreach (var category in categories)
            {
                if (!category.ParentCategoryId.HasValue)
                    continue;

                var children = childCategories.TryGetValue(category.ParentCategoryId.Value).ValueOrElse(new List<int>());
                children.Add(category.Id);
                childCategories[category.ParentCategoryId.Value] = children;
            }

            var transactionsByCategory = transactions.ListDict(t => t.CategoryId.Value);

            foreach (var category in categories)
            {
                var catTransactions = transactionsByCategory.TryGetList(category.Id);
                var catChildren = childCategories.TryGetList(category.Id);

                foreach (var childId in catChildren)
                {
                    var childTransactions = transactionsByCategory.TryGetList(childId);
                    catTransactions.AddRange(childTransactions);
                }

                transactionsByCategory[category.Id] = catTransactions;
            }

            return transactionsByCategory;
        }
    }
}
