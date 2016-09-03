using System;
using Sitecore.Boost.Core.Configuration;

namespace Sitecore.Boost.Core.Caching
{
    public class ClearBoostCache
    {
        public void ClearCache(object sender, EventArgs e)
        {
            BoostContext.Default.PublishAwareCache.Clear();
        }
    }
}