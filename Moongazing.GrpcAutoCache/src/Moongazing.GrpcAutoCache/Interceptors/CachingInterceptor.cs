using Grpc.Core;
using Grpc.Core.Interceptors;
using Moongazing.GrpcAutoCache.Configuration;
using Moongazing.GrpcAutoCache.Core;

namespace Moongazing.GrpcAutoCache.Interceptors
{
    /// <summary>
    /// gRPC interceptor that enables automatic caching for unary server methods.
    /// This interceptor ensures that gRPC method responses are cached 
    /// to improve performance and reduce redundant computations.
    /// </summary>
    public class CachingInterceptor : Interceptor
    {
        private readonly IGrpcCacheManager cacheManager;
        private readonly GrpcCacheOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingInterceptor"/> class.
        /// </summary>
        /// <param name="cacheManager">Instance of the cache manager handling caching logic.</param>
        /// <param name="options">Configuration options for cache behavior.</param>
        public CachingInterceptor(IGrpcCacheManager cacheManager, GrpcCacheOptions options)
        {
            this.cacheManager = cacheManager;
            this.options = options;
        }

        /// <summary>
        /// Overrides the unary server handler to implement caching.
        /// If caching is enabled, responses are stored and retrieved from the cache.
        /// </summary>
        /// <typeparam name="TRequest">The type of the gRPC request.</typeparam>
        /// <typeparam name="TResponse">The type of the gRPC response.</typeparam>
        /// <param name="request">The incoming gRPC request.</param>
        /// <param name="context">The server call context.</param>
        /// <param name="continuation">The next handler in the execution pipeline.</param>
        /// <returns>The cached or newly computed response.</returns>
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // If caching is disabled, process the request normally
            if (!options.EnableCaching)
            {
                return await continuation(request, context);
            }

            // Generate a unique cache key based on method name and request hash
            var cacheKey = $"{context.Method}_{request.GetHashCode()}";

            // Retrieve from cache or execute the method and store the result in cache
            return await cacheManager.GetOrSetAsync(cacheKey, async () =>
            {
                return await continuation(request, context);
            }, options.CacheExpiration);
        }
    }
}
