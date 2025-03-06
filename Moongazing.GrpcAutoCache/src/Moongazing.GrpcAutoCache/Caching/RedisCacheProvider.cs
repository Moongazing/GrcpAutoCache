using Moongazing.GrpcAutoCache.Configuration;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Moongazing.GrpcAutoCache.Caching
{
    /// <summary>
    /// Provides a Redis-based caching mechanism for gRPC services.
    /// Uses <see cref="StackExchange.Redis"/> for storing and retrieving cached responses.
    /// </summary>
    public class RedisCacheProvider : ICacheProvider
    {
        private readonly IDatabase database;
        private readonly GrpcCacheOptions options;
        private readonly ConnectionMultiplexer redisConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisCacheProvider"/> class.
        /// </summary>
        /// <param name="options">Configuration settings for cache behavior, including Redis connection string.</param>
        public RedisCacheProvider(GrpcCacheOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            try
            {
                redisConnection = ConnectionMultiplexer.Connect(this.options.RedisConnectionString);
                database = redisConnection.GetDatabase();
            }
            catch (RedisConnectionException ex)
            {
                throw new InvalidOperationException($"Failed to connect to Redis at {this.options.RedisConnectionString}.", ex);
            }
        }

        /// <summary>
        /// Retrieves a cached item from Redis if available.
        /// </summary>
        /// <typeparam name="T">The type of the cached data.</typeparam>
        /// <param name="key">The unique cache key used to retrieve the data.</param>
        /// <returns>The cached item if it exists; otherwise, returns null.</returns>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = await database.StringGetAsync(key);
                return value.HasValue ? JsonSerializer.Deserialize<T>(value) : default;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheProvider] Error retrieving key '{key}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Stores an item in the Redis cache with an optional expiration duration.
        /// </summary>
        /// <typeparam name="T">The type of data to be cached.</typeparam>
        /// <param name="key">The unique cache key associated with the data.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expiration">
        /// The expiration time for the cached item. If not specified, the default expiration from <see cref="GrpcCacheOptions"/> is used.
        /// </param>
        /// <returns>A task representing the cache storage operation.</returns>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var cacheDuration = expiration ?? options.CacheExpiration;
                var jsonData = JsonSerializer.Serialize(value);
                await database.StringSetAsync(key, jsonData, cacheDuration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheProvider] Error setting key '{key}': {ex.Message}");
            }
        }

        /// <summary>
        /// Removes an item from the Redis cache.
        /// </summary>
        /// <param name="key">The cache key associated with the item to be removed.</param>
        /// <returns>A task representing the cache invalidation operation.</returns>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RedisCacheProvider] Error deleting key '{key}': {ex.Message}");
            }
        }
    }
}
