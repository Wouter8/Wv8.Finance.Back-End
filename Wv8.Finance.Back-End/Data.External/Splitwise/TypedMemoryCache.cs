namespace PersonalFinance.Data.External.Splitwise
{
    using System;
    using System.Runtime.Caching;

    /// <summary>
    /// A class for a <see cref="MemoryCache"/> which is statically typed.
    /// </summary>
    /// <typeparam name="T">The type of the items in the cache.</typeparam>
    public class TypedMemoryCache<T>
    {
        private readonly MemoryCache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypedMemoryCache{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        public TypedMemoryCache(string name)
        {
            this.cache = new MemoryCache(name);
        }

        /// <summary>
        /// Gets a cached value, or retrieves it and adds it to the cache before returning it.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="getFunc">The function to get the item.</param>
        /// <returns>The cached or retrieved item.</returns>
        public T Get(string key, Func<T> getFunc)
        {
            // If the cache already contains the entry, then just return it.
            if (this.cache.Contains(key))
                return (T)this.cache.Get(key);

            // Otherwise retrieve it, add it to the cache, and return it.
            var item = getFunc();
            this.cache.Add(key, item, DateTimeOffset.Now.AddHours(1));

            return item;
        }
    }
}
