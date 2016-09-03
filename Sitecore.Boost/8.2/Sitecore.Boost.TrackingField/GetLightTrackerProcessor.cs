using Sitecore.Analytics.Pipelines.CreateTracker;
using Sitecore.Boost.Core.Configuration;
using Sitecore.Diagnostics;

namespace Sitecore.Boost.LightTrackingField
{
    public class GetLightTrackerProcessor : CreateTrackerProcessor
    {
        public override void Process(CreateTrackerArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            args.Tracker = new LightTracker(BoostContext.Default);
        }
    }
}