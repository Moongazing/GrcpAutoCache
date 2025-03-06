Features

- Automatic caching for gRPC calls
- Supports Redis & MemoryCache
- Works as an Interceptor – no need to modify existing gRPC service logic
- Configurable expiration & cache invalidation
- Boosts gRPC API performance by reducing redundant calls


Installation
 
 -Install via NuGet
 dotnet add package Moongazing.GrpcAutoCache


 -Install from Source Code
git clone https://github.com/Moongazing/Moongazing.GrpcAutoCache/Moongazing.GrpcAutoCache.git
cd Grpc.CacheInterceptor
dotnet build


-Usage

 -Register the Interceptor in Startup.cs


public void ConfigureServices(IServiceCollection services)
{
    services.AddGrpc();
    services.AddSingleton<ICacheProvider, RedisCacheProvider>(); // Use Redis for caching
    services.AddSingleton<IGrpcCacheManager, GrpcCacheManager>();
}

public void Configure(IApplicationBuilder app)
{
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<MyGrpcService>().AddInterceptors<CachingInterceptor>();
    });
}



- Use in a gRPC Service

using Grpc.Core;
using System.Threading.Tasks;

public class MyGrpcService : MyGrpcServiceBase
{
    public override Task<MyResponse> GetData(MyRequest request, ServerCallContext context)
    {
        return Task.FromResult(new MyResponse { Message = "Cached gRPC Response" });
    }
}



-Performance Benchmark

Our tests show up to 40% performance improvement with 100,000+ gRPC requests.


Test Scenario	Cache Disabled	Cache Enabled
10K Requests	220ms	50ms
100K Requests	2.1s	600ms


Configuration

Set Cache Expiration Time

Inside CachingInterceptor.cs, modify the _cacheDuration value:

private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);


Manually Invalidate Cached Data

To remove cached responses for a specific request, call:
await _cacheManager.InvalidateCacheAsync("method_key");

Development & Contribution


Feel free to contribute by forking the repository and opening a pull request:

    git clone https://github.com/Moongazing/Moongazing.GrpcAutoCache/Moongazing.GrpcAutoCache.git
    cd Moongazing.GrpcAutoCache
    dotnet build
    Develop & open a Pull Request! 



Frequently Asked Questions


❓ How does gRPC method caching work?

The interceptor hashes method parameters and responses to generate a unique cache key. The cached response is stored in Redis or MemoryCache, and subsequent requests fetch the cached data instead of processing the request again.
❓ How do I configure caching behavior?

Modify the cache expiration time in CachingInterceptor.cs.
❓ Does this work with all gRPC methods?

Yes! The interceptor works with Unary RPC methods. Support for Streaming RPCs is under development.



License

This project is licensed under the MIT License.
© 2025 - Tunahan Ali Ozturk