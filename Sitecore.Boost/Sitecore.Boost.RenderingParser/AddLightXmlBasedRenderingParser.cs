using Sitecore.Boost.Core.Configuration;
using Sitecore.Mvc.Configuration;
using Sitecore.Mvc.Presentation;
using Sitecore.Pipelines;

namespace Sitecore.Boost.RenderingParser
{
    public class AddLightXmlBasedRenderingParser
    {
        public virtual void Process(PipelineArgs args)
        {
            MvcSettings.RegisterObject<XmlBasedRenderingParser>(() => new LightXmlBasedRenderingParser(BoostContext.Default));
        }
    }
}