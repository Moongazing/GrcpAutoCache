using System;
using System.Threading.Tasks;

namespace Moongazing.GrpcAutoCache.Caching
{
    /// <summary>
    /// Defines a cache provider interface for storing and retrieving cached data.
    /// Supports multiple cache implementations such as MemoryCache and Redis.
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Retrieves a cached item based on the given key.
        /// </summary>
        /// <typeparam name="T">The type of data being retrieved.</typeparam>
        /// <param name="key">The unique cache key associated with the data.</param>
        /// <returns>The cached data if available, otherwise null.</returns>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Stores a data entry in the cache with an optional expiration time.
        /// </summary>
        /// <typeparam name="T">The type of data being cached.</typeparam>
        /// <param name="key">The unique cache key associated with the data.</param>
        /// <param name="value">The value to be stored in the cache.</param>
        /// <param name="expiration">
        /// The duration for which the cached item should be retained.
        /// If null, the default cache expiration policy is applied.
        /// </param>
        /// <returns>A task representing the asynchronous cache storage operation.</returns>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

        /// <summary>
        /// Removes a cached item from the cache storage.
        /// </summary>
        /// <param name="key">The cache key associated with the item to be removed.</param>
        /// <returns>A task representing the cache invalidation operation.</returns>
        Task RemoveAsync(string key);
    }
}
