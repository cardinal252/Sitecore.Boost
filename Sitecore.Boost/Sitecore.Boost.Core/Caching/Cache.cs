

using System.Collections.Concurrent;
using System.Runtime.Caching;

namespace Sitecore.Boost.Core.Caching
{
    public class Cache : ICache
    {
        private const string CacheName = "Sitecore.Boost";

        private static ConcurrentDictionary<string, object> memoryCache = new ConcurrentDictionary<string, object>();

        public T Get<T>(string cacheKey) where T : class
        {
            object result;
            return memoryCache.TryGetValue(cacheKey, out result) ? result as T : default(T);
        }

        public T GetValue<T>(string cacheKey) where T : struct
        {
            object result;
            if (!memoryCache.TryGetValue(cacheKey, out result))
            {
                return default(T);
            }

            return (T)result;
        }

        public void Add<T>(string cacheKey, T item) where T : class
        {
            memoryCache.TryAdd(cacheKey, item);
        }

        public void AddValue<T>(string cacheKey, T item) where T : struct
        {
            memoryCache.TryAdd(cacheKey, item);
        }

        public void Clear()
        {
            memoryCache = new ConcurrentDictionary<string, object>();
        }

        public bool Contains(string cacheKey)
        {
            return memoryCache.ContainsKey(cacheKey);
        }
    }
}
