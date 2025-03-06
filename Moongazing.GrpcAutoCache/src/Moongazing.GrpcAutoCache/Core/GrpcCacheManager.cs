using System;
using System.Threading.Tasks;
using Moongazing.GrpcAutoCache.Caching;

namespace Moongazing.GrpcAutoCache.Core
{
    /// <summary>
    /// Manages caching operations for gRPC requests.
    /// Provides methods to store, retrieve, and invalidate cached data.
    /// </summary>
    public class GrpcCacheManager : IGrpcCacheManager
    {
        private readonly ICacheProvider cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcCacheManager"/> class.
        /// </summary>
        /// <param name="cacheProvider">The cache provider responsible for storing and retrieving cache entries.</param>
        public GrpcCacheManager(ICacheProvider cacheProvider)
        {
            this.cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Retrieves a cached item if it exists; otherwise, fetches new data, caches it, and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of the data to be cached.</typeparam>
        /// <param name="key">The unique cache key for identifying the stored data.</param>
        /// <param name="fetchFunc">The function that fetches new data if the cache is empty.</param>
        /// <param name="expiration">The duration for which the cached item should be retained.</param>
        /// <returns>The cached or newly fetched data.</returns>
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fetchFunc, TimeSpan expiration)
        {
            // Try to get the cached data
            var cachedData = await cacheProvider.GetAsync<T>(key);
            if (cachedData != null) return cachedData;

            // Fetch new data and cache it
            var data = await fetchFunc();
            await cacheProvider.SetAsync(key, data, expiration);
            return data;
        }

        /// <summary>
        /// Removes a cached item from the cache storage.
        /// </summary>
        /// <param name="key">The cache key to invalidate.</param>
        /// <returns>A task representing the cache invalidation operation.</returns>
        public Task InvalidateCacheAsync(string key)
        {
            return cacheProvider.RemoveAsync(key);
        }
    }
}
