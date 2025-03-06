using System;
using System.Threading.Tasks;
using Grpc.Core;
using Moongazing.GrpcAutoCache.Core;
using Moongazing.GrpcAutoCache.Interceptors;
using Moongazing.GrpcAutoCache.Configuration;
using Moq;
using Xunit;

namespace Moongazing.GrpcAutoCache.Tests
{
    /// <summary>
    /// Unit tests for the <see cref="CachingInterceptor"/>.
    /// Ensures caching behavior is correctly applied to gRPC unary calls.
    /// </summary>
    public class CachingInterceptorTests
    {
        private readonly Mock<IGrpcCacheManager> _cacheManagerMock;
        private readonly CachingInterceptor _interceptor;
        private readonly GrpcCacheOptions _options;

        /// <summary>
        /// Initializes test dependencies.
        /// </summary>
        public CachingInterceptorTests()
        {
            _cacheManagerMock = new Mock<IGrpcCacheManager>();
            _options = new GrpcCacheOptions { EnableCaching = true, CacheExpiration = TimeSpan.FromMinutes(5) };
            _interceptor = new CachingInterceptor(_cacheManagerMock.Object, _options);
        }

        /// <summary>
        /// Tests whether the cached response is returned when available.
        /// </summary>
        [Fact]
        public async Task UnaryServerHandler_ReturnsCachedResponse_IfExists()
        {
            // Arrange
            var cacheKey = "TestMethod_123";
            var cachedResponse = new MyResponse { Message = "Cached Response" };

            _cacheManagerMock
                .Setup(c => c.GetOrSetAsync(cacheKey, It.IsAny<Func<Task<MyResponse>>>(), _options.CacheExpiration))
                .ReturnsAsync(cachedResponse);

            var serverCallContext = CreateMockServerCallContext();

            // Act
            var response = await _interceptor.UnaryServerHandler(
                new MyRequest { Id = 123 },
                serverCallContext,
                async (req, ctx) => new MyResponse { Message = "New Response" });

            // Assert
            Assert.Equal("Cached Response", response.Message);
            _cacheManagerMock.Verify(c => c.GetOrSetAsync(cacheKey, It.IsAny<Func<Task<MyResponse>>>(), _options.CacheExpiration), Times.Once);
        }

        /// <summary>
        /// Tests whether the interceptor fetches a new response when the cache is empty.
        /// </summary>
        [Fact]
        public async Task UnaryServerHandler_FetchesNewResponse_WhenCacheIsEmpty()
        {
            // Arrange
            var cacheKey = "TestMethod_456";
            var newResponse = new MyResponse { Message = "New Response" };

            var cachedResponse = new MyResponse { Message = "Cached Response" };

            _cacheManagerMock
                .Setup(c => c.GetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<MyResponse>>>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(cachedResponse);



            var serverCallContext = CreateMockServerCallContext();

            // Act
            var response = await _interceptor.UnaryServerHandler(
                new MyRequest { Id = 456 },
                serverCallContext,
                async (req, ctx) => newResponse);

            // Assert
            Assert.Equal("New Response", response.Message);
            _cacheManagerMock.Verify(c => c.GetOrSetAsync(cacheKey, It.IsAny<Func<Task<MyResponse>>>(), _options.CacheExpiration), Times.Once);
        }

        /// <summary>
        /// Tests whether caching is bypassed when disabled.
        /// </summary>
        [Fact]
        public async Task UnaryServerHandler_BypassesCache_WhenCachingIsDisabled()
        {
            // Arrange
            _options.EnableCaching = false;
            var interceptor = new CachingInterceptor(_cacheManagerMock.Object, _options);
            var serverCallContext = CreateMockServerCallContext();

            var newResponse = new MyResponse { Message = "Direct Response" };

            // Act
            var response = await interceptor.UnaryServerHandler(
                new MyRequest { Id = 789 },
                serverCallContext,
                async (req, ctx) => newResponse);

            // Assert
            Assert.Equal("Direct Response", response.Message);
            _cacheManagerMock.Verify(c => c.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<MyResponse>>>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        /// <summary>
        /// Creates a mock ServerCallContext for testing.
        /// </summary>
        private ServerCallContext CreateMockServerCallContext()
        {
            var mockContext = new Mock<ServerCallContext>();

            mockContext.Setup(ctx => ctx.Method).Returns("TestMethod");
            mockContext.Setup(ctx => ctx.Peer).Returns("Peer");
            mockContext.Setup(ctx => ctx.Host).Returns("localhost");

            return mockContext.Object;
        }
    }

    /// <summary>
    /// Represents a mock gRPC request object.
    /// </summary>
    public class MyRequest
    {
        public int Id { get; set; }
    }

    /// <summary>
    /// Represents a mock gRPC response object.
    /// </summary>
    public class MyResponse
    {
        public string Message { get; set; }
    }
}
