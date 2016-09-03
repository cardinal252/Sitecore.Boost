using Sitecore.Diagnostics;
using Sitecore.Mvc.Pipelines.Response.RenderRendering;

namespace Sitecore.Boost.InitializeProfiling
{
    public class InitializeProfiling : Sitecore.Mvc.Pipelines.Response.RenderRendering.InitializeProfiling
    {
        protected override void StartProfiling(RenderRenderingArgs args)
        {
            // This line is heavy
            // string str = args.Rendering.ToString();
            // Replaced with this as an example
            string str = args.Rendering.Id.ToString();
            Tracer.Info("Starting rendering \"" + str + "\".");
            ++Tracer.Indent;
            Profiler.StartOperation("Render \"" + str + "\".");
        }
    }
}