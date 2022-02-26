namespace PersonalFinance.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Wv8.Core;
    using Wv8.Core.Collections;

    /// <summary>
    /// A class containing extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Groups a list and returns it in a handy dictionary of lists.
        /// </summary>
        /// <param name="list">The list to group.</param>
        /// <param name="selector">The function to select the field to group by.</param>
        /// <typeparam name="TKey">The type of the field to group by.</typeparam>
        /// <typeparam name="TValue">The type of the entries in the list.</typeparam>
        /// <returns>A grouped list in the form of a dictionary.</returns>
        public static Dictionary<TKey, List<TValue>> ListDict<TKey, TValue>(this List<TValue> list, Func<TValue, TKey> selector)
        {
            return list.GroupBy(selector).ToDictionary(g => g.Key, g => g.ToList());
        }

        /// <summary>
        /// Tries to get a list from a dictionary, returns an empty list if the key does not exist.
        /// </summary>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key to look for.</param>
        /// <typeparam name="TKey">The type of the key of the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the entries in the list.</typeparam>
        /// <returns>The found list in the dictionary or an empty list.</returns>
        public static List<TValue> TryGetList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dict, TKey key)
        {
            return dict.TryGetValue(key).ValueOrElse(new List<TValue>());
        }
    }
}
