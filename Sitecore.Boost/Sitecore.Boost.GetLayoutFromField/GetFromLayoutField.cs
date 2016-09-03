using System;
using System.Xml.Linq;
using Sitecore.Boost.Core.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Mvc.Extensions;

namespace Sitecore.Boost.GetLayoutFromField
{
    public class GetFromLayoutField : Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition.GetFromLayoutField
    {
        protected IBoostContext BoostContext { get; private set; }

        public GetFromLayoutField() : this(Core.Configuration.BoostContext.Default)
        {
        }

        public GetFromLayoutField(IBoostContext boostContext)
        {
            BoostContext = boostContext;
        }

        protected override XElement GetFromField(Item item)
        {
            string key = String.Format("{0}_{1}_{2}", item.ID, item.Language.Name, item.Version.Number);
            string fieldValue = BoostContext.PublishAwareCache.Get<string>(key);
            if (fieldValue != null)
            {
                return GetFieldXml(fieldValue);
            }

            Field innerField = new LayoutField(item).InnerField;
            if (innerField == null)
                return null;

            // This line is slow
            fieldValue = LayoutField.GetFieldValue(innerField);
            BoostContext.PublishAwareCache.Add(key, !String.IsNullOrEmpty(fieldValue) ? fieldValue : String.Empty);

            return GetFieldXml(fieldValue);
        }

        private static XElement GetFieldXml(string fieldValue)
        {
            return !fieldValue.IsWhiteSpaceOrNull() ? XDocument.Parse(fieldValue).Root : null;
        }
    }
}