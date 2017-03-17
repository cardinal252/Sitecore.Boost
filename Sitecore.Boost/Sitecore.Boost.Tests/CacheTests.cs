using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Sitecore.Boost.Core.Caching;


namespace Sitecore.Boost.Tests
{
    [TestFixture]
    public class CacheTests
    {
        [Test]
        public void Cache_can_store_objects()
        {
            // Arrange
            Stub stub = new Stub();
            ICacheStatusProvider cacheStatusProvider = Substitute.For<ICacheStatusProvider>();
            cacheStatusProvider.GetCacheStatus().Returns(CacheStatus.Enabled);
            ICache cache = new Cache(cacheStatusProvider);
            const string cacheKey = "cache key";
            cache.Add(cacheKey, stub);

            // Act
            var result = cache.Get<Stub>(cacheKey);

            // Assert
            result.Should().Be(stub);
            cache.Contains(cacheKey).Should().BeTrue();
        }

        [Test]
        public void Cache_when_disabled_returns_default()
        {
            // Arrange
            Stub stub = new Stub();
            ICacheStatusProvider cacheStatusProvider = Substitute.For<ICacheStatusProvider>();
            cacheStatusProvider.GetCacheStatus().Returns(CacheStatus.Disabled);
            ICache cache = new Cache(cacheStatusProvider);
            const string cacheKey = "cache key";
            cache.Add(cacheKey, stub);

            // Act
            var result = cache.Get<Stub>(cacheKey);

            // Assert
            result.Should().NotBe(stub);
            result.Should().BeNull();
            cache.Contains(cacheKey).Should().BeFalse();
        }

        [Test]
        public void Cache_clears_correctly_objects()
        {
            // Arrange
            Stub stub = new Stub();
            ICacheStatusProvider cacheStatusProvider = Substitute.For<ICacheStatusProvider>();
            cacheStatusProvider.GetCacheStatus().Returns(CacheStatus.Enabled);
            ICache cache = new Cache(cacheStatusProvider);
            const string cacheKey = "cache key";
            cache.Add(cacheKey, stub);

            // Act
            cache.Clear();

            // Assert
            cache.Get<Stub>(cacheKey).Should().NotBe(stub);
            cache.Get<Stub>(cacheKey).Should().BeNull();
            cache.Contains(cacheKey).Should().BeFalse();
        }

        internal class Stub
        {
            private string Property { get; set; }
        }
    }
}
