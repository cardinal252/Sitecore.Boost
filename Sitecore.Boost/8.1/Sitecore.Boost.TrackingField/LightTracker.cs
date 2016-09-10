using System;
using System.Threading;
using System.Web;
using Sitecore.Analytics;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Data.DataAccess;
using Sitecore.Analytics.DataAccess;
using Sitecore.Analytics.Pipelines.EnsureSessionContext;
using Sitecore.Analytics.Pipelines.ExcludeRobots;
using Sitecore.Analytics.Pipelines.InitializeTracker;
using Sitecore.Analytics.Pipelines.StartTracking;
using Sitecore.Analytics.Tracking;
using Sitecore.Analytics.Tracking.Diagnostics.PerformanceCounters;
using Sitecore.Analytics.Web;
using Sitecore.Boost.Core.Configuration;
using Sitecore.Common;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Sites;

namespace Sitecore.Boost.LightTrackingField
{
    public class LightTracker : ITracker
    {
        public Contact Contact
        {
            get
            {
                return Session != null 
                    ? Session.Contact 
                    : null;
            }
        }

        public ISessionContext Context
        {
            get
            {
                return Session;
            }
        }

        public Session Session
        {
            get
            {
                return Switcher<Session, Session>.CurrentValue;
            }
        }

        public ICurrentPageContext CurrentPage
        {
            get
            {
                if (Interaction != null)
                    return Interaction.CurrentPage;
                return null;
            }
        }

        public CurrentInteraction Interaction
        {
            get
            {
                if (Session != null)
                    return Session.Interaction;
                return null;
            }
        }

        public bool IsActive
        {
            get
            {
                object obj = Sitecore.Context.Items["SC_TRACKER_ACTIVE"];
                if (obj == null)
                    return false;
                return (bool)obj;
            }
            set
            {
                Sitecore.Context.Items["SC_TRACKER_ACTIVE"] = value;
            }
        }

        public TrackerSamplingBase Sampling { get; private set; }

        public SystemHealthBase SystemHealth
        {
            get
            {
                return DataAdapterManager.SystemHealth;
            }
        }

        protected IBoostContext BoostContext { get; private set; }

        public LightTracker(IBoostContext boostContext)
        {
            BoostContext = boostContext;
            Sampling = new TrackerSampling();
            EnsureSessionContext();
        }

        public void EndTracking()
        {
            IsActive = false;
        }

        public void EndVisit(bool clearVisitor)
        {
            if (!clearVisitor)
                return;
            new ContactKeyCookie("").Invalidate();
        }

        public void StartTracking()
        {
            if (IsActive)
                Log.Debug("Skipping of tracking, tracker is already active", (object)typeof(Analytics.Tracker));
            else if (!Sampling.IsSampling())
            {
                Log.Debug("Session is null or the session is not participate in analytics because was not chosen as a sample for tracking", (object)typeof(Analytics.Tracker));
            }
            else
            {
                SiteContext site = Sitecore.Context.Site;
                if (site != null && !site.Tracking().EnableTracking)
                    Log.Debug("Cannot start tracking, analytics is not enabled for site " + site.Name, (object)typeof(Analytics.Tracker));
                else if (ExcludeRequest())
                {
                    Log.Debug("The request was excluded because the Agent or IP is determined as a robot, see Exclude robots configuration file", (object)typeof(Analytics.Tracker));
                    AnalyticsTrackingCount.CollectionRobotRequests.Increment(1L);
                }
                else if (IgnoreCurrentItem())
                {
                    AnalyticsTrackingCount.CollectionRequestsIgnored.Increment(1L);
                }
                else
                {
                    IsActive = true;
                    StartTrackingArgs args = new StartTrackingArgs()
                    {
                        HttpContext = new HttpContextWrapper(HttpContext.Current)
                    };
                    try
                    {
                        StartTrackingPipeline.Run(args);
                    }
                    catch (ThreadAbortException ex)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        this.IsActive = false;
                        if (!AnalyticsSettings.SuppressTrackingInitializationExceptions)
                            throw new Exception("Failed to start tracking", ex);
                        Log.Error("Cannot start analytics Tracker", ex, typeof(Analytics.Tracker));
                    }
                }
            }
        }

        private bool IgnoreCurrentItem()
        {
            Item item = Sitecore.Context.Item;

            if (item == null)
            {
                return false;
            }

            string cacheKey = String.Format("TrackingField_{0}", item.ID);

            if (BoostContext.PublishAwareCache.Contains(cacheKey))
            {
                bool result = BoostContext.PublishAwareCache.GetValue<bool>(cacheKey);
                return result;
            }

            LightTrackingField trackingField = LightTrackingField.FindTrackingField(item);
            if (trackingField != null)
            {
                var result = trackingField.Ignore;
                BoostContext.PublishAwareCache.AddValue(cacheKey, result);
                return result;
            }

            BoostContext.PublishAwareCache.AddValue(cacheKey, false);
            return false;
        }

        private void EnsureSessionContext()
        {
            InitializeTrackerArgs args = new InitializeTrackerArgs();
            if (HttpContext.Current != null)
                args.HttpContext = new HttpContextWrapper(HttpContext.Current);
            EnsureSessionContextPipeline.Run(args);
            if (args.Session == null)
                return;
            Switcher<Session, Session>.Enter(args.Session);
        }

        private bool ExcludeRequest()
        {
            ExcludeRobotsArgs args = new ExcludeRobotsArgs();
            ExcludeRobotsPipeline.Run(args);
            return args.IsInExcludeList;
        }
    }

}