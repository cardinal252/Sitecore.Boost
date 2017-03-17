using System;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Boost.Core.Configuration;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Boost.RenderingParser
{
    public class LightXmlBasedRenderingParser : XmlBasedRenderingParser
    {
        public LightXmlBasedRenderingParser(IBoostContext boostContext)
        {
            BoostContext = boostContext;
        }

        protected IBoostContext BoostContext { get; private set; }

        public override Rendering Parse(XElement node, bool parseChildNodes)
        {
            string uniqueId = node.GetAttributeValueOrNull("uid");

            // NM: Sure this could be figured out - but this makes a HUGE difference
            string cacheKey = String.Format("LightXmlBasedRenderingParser_{0}_{1}", uniqueId, Context.Item.ID.ToGuid());

            Rendering rendering = BoostContext.PublishAwareCache.Get<Rendering>(cacheKey);
            if (Sitecore.Context.PageMode.IsNormal && !String.IsNullOrEmpty(uniqueId) && rendering != null)
            {
                return rendering;
            }

            // NM: return a light rendering instead.
            rendering = new LightRendering
            {
                ["RenderingType"] = node.Name.LocalName,
                ["Cacheable"] = node.GetAttributeValueOrNull("cac"),
                ["CacheKey"] = node.GetAttributeValueOrNull("ck"),
                ["Cache_Timeout"] = node.GetAttributeValueOrNull("ct"),
                ["Cache_VaryByData"] = node.GetAttributeValueOrNull("vbd"),
                ["Cache_VaryByDevice"] = node.GetAttributeValueOrNull("vbdev"),
                ["Cache_VaryByLogin"] = node.GetAttributeValueOrNull("vbl"),
                ["Cache_VaryByParameters"] = node.GetAttributeValueOrNull("vbp"),
                ["Cache_VaryByQueryString"] = node.GetAttributeValueOrNull("vbqs"),
                ["Cache_VaryByUser"] = node.GetAttributeValueOrNull("vbu"),
                ["DataSource"] = node.GetAttributeValueOrEmpty("ds"),
                ["Model"] = node.GetAttributeValueOrNull("model"),
                ["Placeholder"] = node.GetAttributeValueOrNull("ph"),
                ["RenderingItemPath"] = GetRenderingItemPath(node),
                ["UniqueId"] = uniqueId,
                ["Parameters"] = GetDecodedValue(node.GetAttributeValueOrNull("par"))
            };

            if (node.Name.LocalName == "r")
            {
                rendering["RenderingXml"] = node.ToString();
                XElement xelement = node.Element("rls");
                if (xelement != null)
                    rendering["PersonlizationRules"] = xelement.ToString(SaveOptions.DisableFormatting);
            }
            if (node.Name.LocalName == "d")
            {
                rendering["DeviceId"] = node.GetAttributeValueOrNull("id");
                rendering["LayoutId"] = node.GetAttributeValueOrNull("l");
            }
            node.Attributes().CopyTo(rendering.Properties);
            if (parseChildNodes)
                rendering.ChildRenderings = GetChildRenderings(node).ToList();
            AddRenderingItemProperties(rendering);
            if (rendering["Model"] == null)
                rendering["Model"] = rendering.Parameters["model"];

            if (!String.IsNullOrEmpty(uniqueId))
            {
                BoostContext.PublishAwareCache.Add(cacheKey, rendering);
            }
            return rendering;
        }

        private string GetRenderingItemPath(XElement node)
        {
            string other = node.GetAttributeValueOrNull("id").OrIfEmpty(node.Name.LocalName);
            string attributeValueOrNull = node.GetAttributeValueOrNull("Folder", StringComparison.OrdinalIgnoreCase);
            if (attributeValueOrNull.IsWhiteSpaceOrNull())
                return other;
            return attributeValueOrNull.Append(other, '/');
        }
    }
}