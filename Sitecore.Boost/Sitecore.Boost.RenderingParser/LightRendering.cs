using System;
using System.Collections.Concurrent;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Boost.RenderingParser
{
    public class LightRendering : Rendering
    {
        // NM: Not 100% sure why the renderer for a given rendering isn't cached on a more permanent basis

        public static ConcurrentDictionary<Guid, string> ToStringDictionary = new ConcurrentDictionary<Guid, string>();

        public override string ToString()
        {
            string returnValue;
            if (ToStringDictionary.TryGetValue(Id, out returnValue))
            {
                return returnValue;
            }

            string renderingString = base.ToString();
            ToStringDictionary.TryAdd(Id, renderingString);
            return renderingString;
        }
    }
}