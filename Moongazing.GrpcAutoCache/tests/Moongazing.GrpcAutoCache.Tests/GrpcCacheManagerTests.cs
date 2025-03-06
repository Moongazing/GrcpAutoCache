using Moongazing.GrpcAutoCache.Caching;
using Moongazing.GrpcAutoCache.Core;
using Moq;

namespace Moongazing.GrpcAutoCache.Tests;

public class GrpcCacheManagerTests
{
    private readonly Mock<ICacheProvider> _cacheProviderMock;
    private readonly IGrpcCacheManager _cacheManager;

    public GrpcCacheManagerTests()
    {
        _cacheProviderMock = new Mock<ICacheProvider>();
        _cacheManager = new GrpcCacheManager(_cacheProviderMock.Object);
    }

    [Fact]
    public async Task GetOrSetAsync_ReturnsCachedData_IfExists()
    {
        // Arrange
        var cacheKey = "test_key";
        var cachedValue = "cached_data";
        _cacheProviderMock.Setup(c => c.GetAsync<string>(cacheKey)).ReturnsAsync(cachedValue);

        // Act
        var result = await _cacheManager.GetOrSetAsync(cacheKey, () => Task.FromResult("new_data"), TimeSpan.FromMinutes(5));

        // Assert
        Assert.Equal(cachedValue, result);
        _cacheProviderMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetOrSetAsync_CachesNewData_IfNotExists()
    {
        // Arrange
        var cacheKey = "test_key";
        var newValue = "new_data";
        _cacheProviderMock.Setup(c => c.GetAsync<string>(cacheKey)).ReturnsAsync((string)null);

        // Act
        var result = await _cacheManager.GetOrSetAsync(cacheKey, () => Task.FromResult(newValue), TimeSpan.FromMinutes(5));

        // Assert
        Assert.Equal(newValue, result);
        _cacheProviderMock.Verify(c => c.SetAsync(cacheKey, newValue, TimeSpan.FromMinutes(5)), Times.Once);
    }

    [Fact]
    public async Task InvalidateCacheAsync_RemovesCache()
    {
        // Arrange
        var cacheKey = "test_key";

        // Act
        await _cacheManager.InvalidateCacheAsync(cacheKey);

        // Assert
        _cacheProviderMock.Verify(c => c.RemoveAsync(cacheKey), Times.Once);
    }
}
