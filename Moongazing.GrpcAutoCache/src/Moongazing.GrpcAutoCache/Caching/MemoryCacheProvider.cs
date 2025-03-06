using Microsoft.Extensions.Caching.Memory;
using Moongazing.GrpcAutoCache.Configuration;
using System;
using System.Threading.Tasks;

namespace Moongazing.GrpcAutoCache.Caching
{
    /// <summary>
    /// Provides an in-memory caching mechanism for gRPC services.
    /// This implementation utilizes <see cref="IMemoryCache"/> for storing cached responses.
    /// </summary>
    public class MemoryCacheProvider : ICacheProvider
    {
        private readonly IMemoryCache cache;
        private readonly GrpcCacheOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheProvider"/> class.
        /// </summary>
        /// <param name="cache">The memory cache instance for storing cached data.</param>
        /// <param name="options">Configuration settings for controlling cache behavior.</param>
        public MemoryCacheProvider(IMemoryCache cache, GrpcCacheOptions options)
        {
            this.cache = cache;
            this.options = options;
        }

        /// <summary>
        /// Retrieves a cached item from memory if available.
        /// </summary>
        /// <typeparam name="T">The type of the cached data.</typeparam>
        /// <param name="key">The unique cache key used to retrieve the data.</param>
        /// <returns>The cached item if it exists; otherwise, returns null.</returns>
        public Task<T?> GetAsync<T>(string key)
        {
            // If caching is disabled, return default (null)
            if (!options.EnableCaching)
                return Task.FromResult<T?>(default);

            _ = cache.TryGetValue(key, out T value);
            return Task.FromResult(value);
        }

        /// <summary>
        /// Stores an item in the memory cache with an optional expiration duration.
        /// </summary>
        /// <typeparam name="T">The type of data to be cached.</typeparam>
        /// <param name="key">The unique cache key associated with the data.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expiration">
        /// The expiration time for the cached item. 
        /// If not specified, the default expiration from <see cref="GrpcCacheOptions"/> is used.
        /// </param>
        /// <returns>A task representing the cache storage operation.</returns>
        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            // If caching is disabled, do nothing
            if (!options.EnableCaching) return Task.CompletedTask;

            var cacheDuration = expiration ?? options.CacheExpiration;
            cache.Set(key, value, cacheDuration);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes an item from the cache based on its key.
        /// </summary>
        /// <param name="key">The cache key associated with the item to be removed.</param>
        /// <returns>A task representing the cache removal operation.</returns>
        public Task RemoveAsync(string key)
        {
            cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
