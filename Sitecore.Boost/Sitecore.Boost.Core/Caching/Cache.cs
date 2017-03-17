using System.Collections.Concurrent;

namespace Sitecore.Boost.Core.Caching
{
    public class Cache : ICache
    {
        private static ConcurrentDictionary<string, object> memoryCache = new ConcurrentDictionary<string, object>();

        public Cache(ICacheStatusProvider cacheStatusProvider)
        {
            CacheStatusProvider = cacheStatusProvider;
        }

        protected ICacheStatusProvider CacheStatusProvider { get; }

        public T Get<T>(string cacheKey) where T : class
        {
            if (CacheStatusProvider.GetCacheStatus() == CacheStatus.Disabled)
            {
                return default(T);
            }

            object result;
            return memoryCache.TryGetValue(cacheKey, out result) ? result as T : default(T);
        }

        public T GetValue<T>(string cacheKey) where T : struct
        {
            if (CacheStatusProvider.GetCacheStatus() == CacheStatus.Disabled)
            {
                return default(T);
            }

            object result;
            if (!memoryCache.TryGetValue(cacheKey, out result))
            {
                return default(T);
            }

            return (T)result;
        }

        public void Add<T>(string cacheKey, T item) where T : class
        {
            if (CacheStatusProvider.GetCacheStatus() == CacheStatus.Disabled)
            {
                return;
            }

            memoryCache.TryAdd(cacheKey, item);
        }

        public void AddValue<T>(string cacheKey, T item) where T : struct
        {
            if (CacheStatusProvider.GetCacheStatus() == CacheStatus.Disabled)
            {
                return;
            }

            memoryCache.TryAdd(cacheKey, item);
        }

        public void Clear()
        {
            memoryCache = new ConcurrentDictionary<string, object>();
        }

        public bool Contains(string cacheKey)
        {
            return CacheStatusProvider.GetCacheStatus() != CacheStatus.Disabled && memoryCache.ContainsKey(cacheKey);
        }
    }
}
