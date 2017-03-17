using Sitecore.ContentTesting.Inspectors;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Xml.Patch;

namespace Sitecore.Boost.RenderingParser
{
    public class GetFromFinalLayout : LightGetFromLayoutBase
    {
        protected override TestingRenderingInspector CreateInspector(Item item)
        {
            string finalLayoutXml = item[FieldIDs.FinalLayoutField];
            if (!string.IsNullOrEmpty(finalLayoutXml) && XmlPatchUtils.IsXmlPatch(finalLayoutXml))
            {
                finalLayoutXml = XmlDeltas.ApplyDelta("<r/>", finalLayoutXml);
            }

            LayoutDefinition layoutDefinition1 = LayoutDefinition.Parse(finalLayoutXml);
            LayoutDefinition layoutDefinition2 = LayoutDefinition.Parse(LayoutField.GetFieldValue(item.Fields[FieldIDs.FinalLayoutField]));
            foreach (DeviceDefinition device1 in layoutDefinition1.Devices)
            {
                DeviceDefinition device2 = layoutDefinition2.GetDevice(device1.ID);
                if (device2 != null)
                {
                    for (int index = 0; index < device1.Renderings.Count; ++index)
                    {
                        RenderingDefinition rendering = device1.Renderings[index] as RenderingDefinition;
                        RenderingDefinition renderingByUniqueId = device2.GetRenderingByUniqueId(rendering.UniqueId);
                        if (renderingByUniqueId != null)
                        {
                            device1.Renderings[index] = renderingByUniqueId;
                        }
                    }
                }
            }
            string xml = layoutDefinition1.ToXml();
            return new TestingRenderingInspector(item, xml);
        }
    }
}