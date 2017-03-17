using System;
using System.Web;

namespace Sitecore.Boost.Core.Caching
{
    public class DefaultCacheStatusProvider : ICacheStatusProvider
    {
        public CacheStatus GetCacheStatus()
        {
            if (HttpContext.Current?.Request?.Url.PathAndQuery.IndexOf("/sitecore", StringComparison.OrdinalIgnoreCase) >= 0 ||
                !Context.PageMode.IsNormal)
            {
                return CacheStatus.Disabled;
            }

            return CacheStatus.Enabled;
        }
    }
}