using System.Runtime.InteropServices.ComTypes;

namespace Sitecore.Boost.Core.Caching
{
    public interface ICache
    {
        T Get<T>(string cacheKey) where T : class;

        T GetValue<T>(string cacheKey) where T : struct;

        void Add<T>(string cacheKey, T item) where T : class;

        void AddValue<T>(string cacheKey, T item) where T : struct;

        void Clear();

        bool Contains(string cacheKey);
    }
}
