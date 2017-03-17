using Sitecore.ContentTesting.Inspectors;
using Sitecore.Data.Items;

namespace Sitecore.Boost.RenderingParser
{
    public class GetFromVisualisation : LightGetFromLayoutBase
    {
        protected override TestingRenderingInspector CreateInspector(Item item)
        {
            return new TestingRenderingInspector(item);
        }
    }
}