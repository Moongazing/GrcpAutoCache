namespace Moongazing.GrpcAutoCache.Core
{
    /// <summary>
    /// Defines a caching manager interface for gRPC services.
    /// Provides methods for retrieving, storing, and invalidating cached data.
    /// </summary>
    public interface IGrpcCacheManager
    {
        /// <summary>
        /// Retrieves a cached item if available; otherwise, fetches new data, caches it, and returns the result.
        /// </summary>
        /// <typeparam name="T">The type of data being cached.</typeparam>
        /// <param name="key">The unique cache key associated with the data.</param>
        /// <param name="fetchFunc">A function that retrieves fresh data if the cache is empty.</param>
        /// <param name="expiration">The duration for which the cached item should be retained.</param>
        /// <returns>The cached data if available, otherwise the newly fetched data.</returns>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> fetchFunc, TimeSpan expiration);

        /// <summary>
        /// Invalidates and removes a cached item from the storage.
        /// </summary>
        /// <param name="key">The cache key associated with the item to be removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task InvalidateCacheAsync(string key);
    }
}
