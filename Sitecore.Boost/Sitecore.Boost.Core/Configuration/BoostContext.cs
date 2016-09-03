using System;
using Sitecore.Boost.Core.Caching;

namespace Sitecore.Boost.Core.Configuration
{
    public class BoostContext : IBoostContext
    {
        private static readonly IBoostContext boostContext = new BoostContext();

        public BoostContext()
        {
            PublishAwareCache = new Cache();
        }

        public ICache PublishAwareCache { get; set; }

        public static IBoostContext Default
        {
            get
            {
                return boostContext;
            }
        }

        public static void SetupDefault(Action<IBoostContext> setupAction)
        {
            setupAction(boostContext);
        }
    }
}
