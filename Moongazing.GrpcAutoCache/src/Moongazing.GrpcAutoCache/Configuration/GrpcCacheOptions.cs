using System;

namespace Moongazing.GrpcAutoCache.Configuration;

/// <summary>
/// Configuration options for gRPC caching.
/// Allows customization of caching behavior, provider selection, expiration policies, and logging.
/// </summary>
public class GrpcCacheOptions
{
    /// <summary>
    /// Gets or sets the default cache expiration time.
    /// Default: 5 minutes.
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Determines whether caching is enabled.
    /// If false, all caching operations will be bypassed.
    /// Default: true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Defines the cache provider to be used.
    /// Available options:
    /// - "Memory" (default) - Uses in-memory caching.
    /// - "Redis" - Uses Redis for distributed caching.
    /// Default: "Memory".
    /// </summary>
    public string CacheProvider { get; set; } = "Memory"; // Options: "Memory", "Redis"

    /// <summary>
    /// Determines whether logging is enabled for cache operations.
    /// If true, cache actions such as set, get, and remove will be logged for debugging purposes.
    /// Default: false.
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// The Redis connection string, used when <see cref="CacheProvider"/> is set to "Redis".
    /// Default: "localhost".
    /// Example: "redis-server:6379" or "localhost:6379".
    /// </summary>
    public string RedisConnectionString { get; set; } = "localhost";
}
