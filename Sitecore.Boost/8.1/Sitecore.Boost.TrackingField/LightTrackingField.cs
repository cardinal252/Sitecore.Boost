using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sitecore.Analytics.Data;
using Sitecore.Analytics.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Links;

namespace Sitecore.Boost.Tracker
{
    public class LightTrackingField : CustomField
    {
        private List<ContentProfile> profiles = new List<ContentProfile>();
        private const string presetFieldName = "Profile Card Value";

        // NM: Added dictionary to check against.
        private readonly Dictionary<string, ContentProfile> profilesDictionary =
            new Dictionary<string, ContentProfile>(); 
        private XDocument document;

        public static string PresetFieldName
        {
            get
            {
                return Assert.ResultNotNull<string>("Profile Card Value");
            }
        }

        public IEnumerable<string> CampaignIds
        {
            get
            {
                return Root.Elements("campaign").Select(e => e.Attribute("id")).Where(a => a != null).Select(a => a.Value);
            }
        }

        public IEnumerable<string> CampaignNames
        {
            get
            {
                return Root.Elements("campaign").Select(e => e.Attribute("title")).Where(a => a != null).Select(a => a.Value);
            }
        }

        public IEnumerable<CampaignItem> Campaigns
        {
            get
            {
                AnalyticsItems analytics = Analytics.Tracker.DefinitionItems;
                return Root.Elements("campaign").Select(e => e.Attribute("id")).Where(a => a != null).Select(a => analytics.Campaigns[new ID(a.Value)]);
            }
        }

        public IEnumerable<string> EventIds
        {
            get
            {
                return Root.Elements("event").Select(e => e.Attribute("id")).Where(a => a != null).Select(a => a.Value);
            }
        }

        public IEnumerable<string> EventNames
        {
            get
            {
                return Root.Elements("event").Select(e => e.Attribute("name")).Where(a => a != null).Select(a => a.Value);
            }
        }

        public IEnumerable<PageEventData> Events
        {
            get
            {
                AnalyticsItems analytics = Analytics.Tracker.DefinitionItems;
                return Root.Elements("event").Select(e => new PageEventData(analytics, new Guid(e.GetAttributeValue("id")))
                {
                    Name = e.GetAttributeValue("name"),
                    Data = e.GetAttributeValue("data")
                });
            }
        }

        public bool Ignore
        {
            get
            {
                XAttribute xattribute = Root.Attribute("ignore");
                if (xattribute != null)
                    return xattribute.Value == "1";
                return false;
            }
        }

        public ContentProfile[] Profiles
        {
            get
            {
                return Assert.ResultNotNull(profiles.ToArray());
            }
        }

        public XElement Root
        {
            get
            {
                return Assert.ResultNotNull(document.Element("tracking"));
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (Root.Attributes().FirstOrDefault() == null)
                    return Root.Elements().FirstOrDefault() == null;
                return false;
            }
        }

        protected XDocument Xml
        {
            get
            {
                return document;
            }
        }

        public LightTrackingField(Field innerField)
          : base(innerField)
        {
            Assert.ArgumentNotNull(innerField, "innerField");
            Initialize();
        }

        public LightTrackingField(Field innerField, string runtimeValue)
          : base(innerField, runtimeValue)
        {
            Assert.ArgumentNotNull(innerField, "innerField");
            Assert.ArgumentNotNull(runtimeValue, "runtimeValue");
            Initialize();
        }

        internal static LightTrackingField FindTrackingField(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            LightTrackingField trackingField = GetTrackingField(item);
            if (trackingField != null)
                return trackingField;
            TemplateItem template = item.Template;
            if (template == null)
                return null;
            return GetTrackingField(template.InnerItem);
        }

        internal static bool HasTracking(Item item)
        {
            LightTrackingField trackingField = FindTrackingField(item);
            return trackingField != null && !trackingField.IsEmpty;
        }

        public void AcceptChanges()
        {
            Value = GetFieldValue();
        }

        public string GetFieldValue()
        {
            InitializeDocument();
            XDocument xdocument = document;
            XElement xelement1 = xdocument.Element("tracking");
            if (xelement1 == null)
            {
                xelement1 = new XElement("tracking");
                xdocument.Add(xelement1);
            }
            List<XElement> list = xelement1.Elements("profile").ToList();
            for (int index = 0; index < list.Count; ++index)
                list[index].Remove();
            foreach (XElement xelement2 in Enumerable.Where(Profiles, profile => profile.SaveToField).Select(profile => profile.ToXElement()))
                xelement1.Add(xelement2);
            return xdocument.ToString();
        }

        public ContentProfile GetProfile(Item profileItem)
        {
            Assert.ArgumentNotNull(profileItem, "profileItem");
            return Profiles.FirstOrDefault(profile => profile.ProfileID == profileItem.ID);
        }

        public bool HasCampaign(string title)
        {
            return Enumerable.Any(Root.Elements("campaign").Select(e => e.Attribute("title")), a =>
            {
                if (a != null)
                    return string.Compare(a.Value, title, StringComparison.InvariantCultureIgnoreCase) == 0;
                return false;
            });
        }

        public bool HasCampaign(Guid id)
        {
            return Root.Elements("campaign").Select(e => e.Attribute("id")).Any(a =>
            {
                if (a != null)
                    return new Guid(a.Value) == id;
                return false;
            });
        }

        public bool HasEvent(string name)
        {
            Assert.ArgumentNotNull(name, "name");
            return Enumerable.Any(Enumerable.Select(Root.Elements("event"), e => e.Attribute("name")), a =>
            {
                if (a != null)
                    return string.Compare(a.Value, name, StringComparison.InvariantCultureIgnoreCase) == 0;
                return false;
            });
        }

        public bool HasGoal(string name)
        {
            Assert.ArgumentNotNull(name, "name");
            return HasEvent(name);
        }

        public static void UpdateKeyValues(ContentProfile profile)
        {
            Assert.ArgumentNotNull(profile, "profile");
            Assert.IsFalse(profile.ProfileID == ID.Null, "profile ID");
            if (profile.Presets == null || profile.Presets.Count == 0)
                return;
            Item profileItem = profile.GetProfileItem();
            if (profileItem == null)
                return;
            Item presetsFolder = ProfileUtil.GetPresetsFolder(profileItem);
            if (presetsFolder == null)
                return;
            SimpleProfileKeyCalculator profileKeyCalculator = null;
            string strA = presetsFolder["Authoring Selection"];
            if (string.Compare(strA, "{C4960DD5-8B07-4025-8E48-57C3BC578CE1}", StringComparison.InvariantCultureIgnoreCase) == 0)
                profileKeyCalculator = new SimpleProfileKeyCalculator(profile);
            else if (string.Compare(strA, "{DF9486E3-C239-406E-83DD-7A30BEF2599D}", StringComparison.InvariantCultureIgnoreCase) == 0)
                profileKeyCalculator = new MultipleProfileKeyCalculator(profile);
            else if (string.Compare(strA, "{2DE135D7-FB39-42B2-B10A-13CB4285E5C5}", StringComparison.InvariantCultureIgnoreCase) == 0)
                profileKeyCalculator = new MultipleWithPercentageProfileKeyCalculator(profile);
            if (profileKeyCalculator == null)
                return;
            profileKeyCalculator.Calculate();
        }

        internal static LightTrackingField GetTrackingField(Item item)
        {
            Assert.ArgumentNotNull(item, "item");
            Field innerField = item.Fields["__Tracking"];
            if (innerField != null)
                return new LightTrackingField(innerField);
            return null;
        }

        private XElement Find(string elementName, ID id, string name)
        {
            XElement xelement1 = null;
            foreach (XElement xelement2 in Root.Elements(elementName))
            {
                XAttribute xattribute1 = xelement2.Attribute("id");
                if (xattribute1 != null && xattribute1.Value == id.ToString())
                    return xelement2;
                XAttribute xattribute2 = xelement2.Attribute("name") ?? xelement2.Attribute("title");
                if (xattribute2 != null && xattribute2.Value == name)
                    xelement1 = xelement2;
            }
            return xelement1;
        }

        public override void Relink(ItemLink link, Item newLink)
        {
            Item targetItem = link.GetTargetItem();
            if (targetItem == null)
                return;
            ID templateId = targetItem.TemplateID;
            if (templateId != newLink.TemplateID)
                throw new Exception(Translate.Text("Cannot relink to an item with a different template."));
            string elementType = GetElementType(templateId);
            if (elementType == null)
                return;
            XElement xelement = Find(elementType, targetItem.ID, targetItem.Name);
            if (xelement != null)
            {
                xelement.SetAttributeValue("id", newLink.ID.ToString());
                xelement.SetAttributeValue((elementType == "campaign" ? "title" : "name"), newLink.Name);
            }
            AcceptChanges();
        }

        private string GetElementType(ID templateID)
        {
            string str = null;
            if (templateID == CampaignItem.TemplateID)
                str = "campaign";
            if (templateID == GoalItem.TemplateID || templateID == PageEventItem.TemplateID)
                str = "event";
            if (templateID == ProfileItem.TemplateID)
                str = "profile";
            return str;
        }

        protected internal virtual void UpdateLink(Item item, ItemChanges changes)
        {
            PropertyChange propertyChange;
            if (!changes.Properties.TryGetValue("name", out propertyChange))
                return;
            string name = propertyChange.OriginalValue as string;
            string str = propertyChange.Value as string;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(str))
                return;
            string elementType = GetElementType(item.TemplateID);
            if (elementType == null)
                return;
            XElement xelement = Find(elementType, item.ID, name);
            if (xelement != null)
            {
                xelement.SetAttributeValue("id", item.ID);
                xelement.SetAttributeValue((elementType == "campaign" ? "title" : "name"), str);
            }
            AcceptChanges();
        }

        public override void RemoveLink(ItemLink itemLink)
        {
            Item targetItem = itemLink.GetTargetItem();
            if (targetItem == null)
                return;
            string elementType = GetElementType(targetItem.TemplateID);
            if (elementType == null)
                return;
            XElement xelement = Find(elementType, targetItem.ID, targetItem.Name);
            if (xelement == null)
                return;
            xelement.Remove();
            AcceptChanges();
        }

        public override void ValidateLinks(LinksValidationResult result)
        {
            foreach (string targetPath in CampaignIds)
            {
                CampaignItem campaignItem = null;
                ID result1;
                if (ID.TryParse(targetPath, out result1))
                    campaignItem = Analytics.Tracker.DefinitionItems.Campaigns[result1];
                if (campaignItem == null)
                    result.AddBrokenLink(targetPath);
                else
                    result.AddValidLink(campaignItem.InnerItem, campaignItem.InnerItem.Paths.Path);
            }
            foreach (PageEventData pageEventData in Events)
            {
                PageEventItem pageEventItem = null;
                if (pageEventData.PageEventDefinitionId != Guid.Empty)
                    pageEventItem = Analytics.Tracker.DefinitionItems.Goals[pageEventData.PageEventDefinitionId] ?? Analytics.Tracker.DefinitionItems.PageEvents[pageEventData.PageEventDefinitionId];
                if (pageEventItem == null && pageEventData.Name != null)
                    pageEventItem = Analytics.Tracker.DefinitionItems.Goals[pageEventData.Name] ?? Analytics.Tracker.DefinitionItems.PageEvents[pageEventData.Name];
                if (pageEventItem == null)
                    result.AddBrokenLink(FileUtil.MakePath(Analytics.Tracker.DefinitionItems.Goals.Path, pageEventData.Name, '/'));
                else
                    result.AddValidLink(pageEventItem.InnerItem, pageEventItem.InnerItem.Paths.Path);
            }
            foreach (ContentProfile contentProfile in Profiles.Where(p => p.SaveToField))
                result.AddValidLink(contentProfile.Definition.InnerItem, contentProfile.Definition.InnerItem.Paths.Path);
        }

        protected static void ProcessProfileKeys(ContentProfile profile, ProfileItem profileItem)
        {
            Assert.ArgumentNotNull(profile, "profile");
            Assert.ArgumentNotNull(profileItem, "profileItem");
            Assert.IsFalse(profile.ProfileID == ID.Null, "profile ID");

            foreach (ProfileKeyItem profileKeyItem in profileItem.Keys.ToList())
            {
                ProfileKeyItem keyItem = profileKeyItem;
                // NM: Check to see if there are any keys
                if (profile.Keys.Length == 0)
                {
                    continue;
                }

                if (profile.Keys.FirstOrDefault(k => string.Compare(k.Key, keyItem.Name, StringComparison.OrdinalIgnoreCase) == 0) == null)
                {
                    ContentProfileKeyData key = new ContentProfileKeyData(keyItem)
                    {
                        Value = keyItem.GetDefaultValue()
                    };
                    profile.AddKey(key);
                }
            }
            foreach (ContentProfileKeyData key in profile.Keys.Where(key => key.ProfileDefinitionId == Guid.Empty))
                profile.RemoveKey(key);
            UpdateKeyValues(profile);
        }

        protected void Initialize()
        {
            InitializeDocument();
            InitializeProfiles();
        }

        protected void InitializeDocument()
        {
            if (document != null)
                return;
            string text = Value;
            document = string.IsNullOrEmpty(text) ? new XDocument(new XElement("tracking")) : XDocument.Parse(text);
        }

        protected void InitializeProfiles()
        {
            profiles = new List<ContentProfile>();
            AnalyticsItems analytics = InnerField.Database.Analytics(InnerField.Language);
            if (!string.IsNullOrEmpty(Value))
            {
                foreach (XElement node in Root.Elements("profile"))
                {
                    // NM: Don't know the usage ouside of the tracking field, but internally, it only ever uses first or default. Dictionary should be quicker

                    ContentProfile contentProfile = ContentProfile.Parse(node, InnerField.Item, analytics);
                    if (contentProfile != null)
                    {
                        contentProfile.IsSavedInField = true;
                        profiles.Add(contentProfile);
                        string profileKey = contentProfile.Name.ToLowerInvariant();
                        if (!profilesDictionary.ContainsKey(profileKey))
                        {
                            profilesDictionary.Add(profileKey, contentProfile);
                        }
                    }
                }
            }
            NormalizeProfiles(analytics);
        }

        protected void NormalizeProfiles(AnalyticsItems analytics)
        {
            Assert.ArgumentNotNull(analytics, "analytics");
            foreach (ProfileItem profileItem1 in analytics.Profiles)
            {
                ProfileItem profileItem = profileItem1;
                ContentProfile profile = null;
                // NM: Replaced the list first or default with a dictionary contains key
                string profileKey = profileItem.Name;
                if (profilesDictionary.ContainsKey(profileKey))
                {
                    profile = profilesDictionary[profileKey];
                }
                if (profile == null)
                {
                    profile = new ContentProfile(profileItem) {IsSavedInField = false};
                    profiles.Add(profile);
                    profilesDictionary.Add(profileKey, profile);
                }
                ProcessProfileKeys(profile, profileItem);
            }
        }

        private class SimpleProfileKeyCalculator
        {
            private readonly ContentProfile profile;

            protected ContentProfile Profile
            {
                get
                {
                    return Assert.ResultNotNull(profile);
                }
            }

            public SimpleProfileKeyCalculator(ContentProfile profile)
            {
                Assert.ArgumentNotNull(profile, "profile");
                this.profile = profile;
            }

            public virtual void Calculate()
            {
                if (Profile.Presets == null || Profile.Presets.Count == 0)
                    return;
                if (Profile.Presets.Count > 1)
                    profile.Presets = new Dictionary<string, float>
                    {
            {
              Profile.Presets.Keys.First(),
              100f
            }
          };
                string presetKey = Profile.Presets.Keys.First();
                if (string.IsNullOrEmpty(presetKey))
                    return;
                ContentProfile presetProfileData = GetPresetProfileData(Profile, presetKey);
                if (presetProfileData == null)
                    return;
                foreach (ContentProfileKeyData contentProfileKeyData1 in presetProfileData.Keys)
                {
                    string keyId = contentProfileKeyData1.Key;
                    ContentProfileKeyData contentProfileKeyData2 = Profile.Keys.FirstOrDefault(k => string.Compare(k.Key, keyId, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (contentProfileKeyData2 != null)
                        contentProfileKeyData2.Value = contentProfileKeyData1.Value;
                }
            }

            protected virtual ContentProfile GetPresetProfileData(ContentProfile profile, string presetKey)
            {
                Assert.ArgumentNotNull(profile, "profile");
                Assert.ArgumentNotNull(presetKey, "presetKey");
                LightTrackingField presetTrackingField = GetPresetTrackingField(profile, presetKey);
                if (presetTrackingField == null)
                    return null;
                return presetTrackingField.Profiles.FirstOrDefault(p => p.ProfileID == profile.ProfileID);
            }

            protected virtual LightTrackingField GetPresetTrackingField(ContentProfile profile, string presetKey)
            {
                Assert.ArgumentNotNull(profile, "profile");
                Assert.ArgumentNotNull(presetKey, "presetKey");
                Item presetItem = profile.GetPresetItem(presetKey);
                if (presetItem == null)
                    return null;
                return GetTrackingField(profile, presetItem);
            }

            protected virtual LightTrackingField GetTrackingField(ContentProfile profile, Item presetItem)
            {
                Assert.ArgumentNotNull(profile, "profile");
                Assert.ArgumentNotNull(presetItem, "presetItem");
                Field innerField = presetItem.Fields[TrackingField.PresetFieldName];
                if (innerField == null)
                    return null;
                return new LightTrackingField(innerField);
            }
        }

        private class MultipleProfileKeyCalculator : SimpleProfileKeyCalculator
        {
            public MultipleProfileKeyCalculator(ContentProfile profile)
              : base(profile)
            {
                Assert.ArgumentNotNull(profile, "profile");
            }

            public override void Calculate()
            {
                if (Profile.Presets == null || Profile.Presets.Count == 0)
                    return;
                int num = 0;
                Dictionary<string, float> dictionary1 = new Dictionary<string, float>();
                foreach (string presetKey in Profile.Presets.Keys)
                {
                    if (string.IsNullOrEmpty(presetKey))
                        return;
                    ContentProfile presetProfileData = GetPresetProfileData(Profile, presetKey);
                    if (presetProfileData == null)
                        return;
                    ++num;
                    foreach (ContentProfileKeyData contentProfileKeyData in presetProfileData.Keys)
                    {
                        string key = contentProfileKeyData.Key.ToLowerInvariant();
                        if (!dictionary1.ContainsKey(key))
                            dictionary1.Add(key, 0.0f);
                        Dictionary<string, float> dictionary2;
                        string index;
                        (dictionary2 = dictionary1)[index = key] = dictionary2[index] + contentProfileKeyData.Value;
                    }
                }
                if (num <= 0)
                    return;
                List<string> list = new List<string>(dictionary1.Keys);
                foreach (string str in list)
                {
                    Dictionary<string, float> dictionary2;
                    string index;
                    (dictionary2 = dictionary1)[index = str] = dictionary2[index] / num;
                }
                foreach (string str in list)
                {
                    string keyId = str;
                    ContentProfileKeyData contentProfileKeyData = Profile.Keys.FirstOrDefault(k => string.Compare(k.Key, keyId, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (contentProfileKeyData != null)
                        contentProfileKeyData.Value = dictionary1[keyId];
                }
            }
        }

        private class MultipleWithPercentageProfileKeyCalculator : SimpleProfileKeyCalculator
        {
            public MultipleWithPercentageProfileKeyCalculator(ContentProfile profile)
              : base(profile)
            {
                Assert.ArgumentNotNull(profile, "profile");
            }

            public override void Calculate()
            {
                if (Profile.Presets == null || Profile.Presets.Count == 0)
                    return;
                int num1 = 0;
                Dictionary<string, float> dictionary1 = new Dictionary<string, float>();
                foreach (string presetKey in Profile.Presets.Keys)
                {
                    if (string.IsNullOrEmpty(presetKey))
                        return;
                    ContentProfile presetProfileData = GetPresetProfileData(Profile, presetKey);
                    if (presetProfileData == null)
                        return;
                    ++num1;
                    float num2 = Profile.Presets[presetKey];
                    foreach (ContentProfileKeyData contentProfileKeyData in presetProfileData.Keys)
                    {
                        string key = contentProfileKeyData.Key.ToLowerInvariant();
                        if (!dictionary1.ContainsKey(key))
                            dictionary1.Add(key, 0.0f);
                        Dictionary<string, float> dictionary2;
                        string index;
                        (dictionary2 = dictionary1)[index = key] = dictionary2[index] + (float)(contentProfileKeyData.Value * (double)num2 / 100.0);
                    }
                }
                if (num1 <= 0)
                    return;
                foreach (string str in new List<string>(dictionary1.Keys))
                {
                    string keyId = str;
                    ContentProfileKeyData contentProfileKeyData = Profile.Keys.FirstOrDefault(k => string.Compare(k.Key, keyId, StringComparison.InvariantCultureIgnoreCase) == 0);
                    if (contentProfileKeyData != null)
                        contentProfileKeyData.Value = dictionary1[keyId];
                }
            }
        }

        public class PageEventData
        {
            private readonly AnalyticsItems analytics;

            public string Data { get; set; }

            public PageEventItem DefinitionItem
            {
                get
                {
                    PageEventItem pageEventItem = null;
                    if (PageEventDefinitionId != Guid.Empty)
                        pageEventItem = analytics.Goals[PageEventDefinitionId] ?? analytics.PageEvents[PageEventDefinitionId];
                    if (pageEventItem == null)
                        pageEventItem = Name == null ? null : analytics.Goals[Name] ?? analytics.PageEvents[Name];
                    return pageEventItem;
                }
            }

            public string Name { get; set; }

            public Guid PageEventDefinitionId { get; set; }

            public PageEventData(AnalyticsItems analytics)
              : this(analytics, Guid.Empty)
            {
            }

            public PageEventData(AnalyticsItems analytics, Guid pageEventDefinitionId)
            {
                Assert.ArgumentNotNull(analytics, "analytics");
                this.analytics = analytics;
                PageEventDefinitionId = pageEventDefinitionId;
            }
        }
    }
}
