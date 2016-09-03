using Sitecore.Boost.Core.Caching;

namespace Sitecore.Boost.Core.Configuration
{
    public interface IBoostContext
    {
        ICache PublishAwareCache { get; set; }
    }
}
