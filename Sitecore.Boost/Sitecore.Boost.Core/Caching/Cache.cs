

using System.Runtime.Caching;

namespace Sitecore.Boost.Core.Caching
{
    public class Cache : ICache
    {
        private const string CacheName = "Sitecore.Boost";

        private static MemoryCache memoryCache = new MemoryCache(CacheName);

        public T Get<T>(string cacheKey) where T : class
        {
            return memoryCache.Get(cacheKey) as T;
        }

        public T GetValue<T>(string cacheKey) where T : struct
        {
            var result = memoryCache.Get(cacheKey);
            if (result == null)
            {
                return default(T);
            }

            return (T)result;
        }

        public void Add<T>(string cacheKey, T item) where T : class
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            memoryCache.Add(cacheKey, item, policy);
        }

        public void AddValue<T>(string cacheKey, T item) where T : struct
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            memoryCache.Add(cacheKey, item, policy);
        }

        public void Clear()
        {
            memoryCache = new MemoryCache(CacheName);
        }

        public bool Contains(string cacheKey)
        {
            return memoryCache.Contains(cacheKey);
        }
    }
}
