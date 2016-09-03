using Sitecore.Layouts;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Boost.Platform.Code
{
    public class SetCacheability : Sitecore.Mvc.Pipelines.Response.RenderRendering.SetCacheability
    {
        protected virtual string ClearOnIndexUpdateCacheKey
        {
            get
            {
                return "ClearOnIndexUpdate";
            }
        }

        protected override bool IsCacheable(Rendering rendering, RenderRenderingArgs args)
        {
            if (rendering.RenderingItem != null && rendering.RenderingItem.Caching != null)
            {
                rendering.Caching.Cacheable = rendering.RenderingItem.Caching.Cacheable;
            }

            bool flag = rendering.Caching.Cacheable && DoesContextAllowCaching(args);
            if (flag)
                AddCachingSettings(rendering);
            return flag;
        }

        protected virtual void AddCachingSettings(Rendering rendering)
        {
            RenderingCaching renderingCaching = rendering.RenderingItem.Caching;
            rendering.Caching.VaryByData = renderingCaching.VaryByData;
            rendering.Caching.VaryByDevice = renderingCaching.VaryByDevice;
            rendering.Caching.VaryByLogin = renderingCaching.VaryByLogin;
            rendering.Caching.VaryByParameters = renderingCaching.VaryByParm;
            rendering.Caching.VaryByQueryString = renderingCaching.VaryByQueryString;
            rendering.Caching.VaryByUser = renderingCaching.VaryByUser;
            rendering[ClearOnIndexUpdateCacheKey] = renderingCaching.ClearOnIndexUpdate ? "1" : string.Empty;
        }
    }
}
