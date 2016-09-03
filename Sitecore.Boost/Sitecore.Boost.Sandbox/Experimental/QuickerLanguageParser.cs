using System;
using System.Collections.Generic;
using Sitecore.Data.Managers;
using Sitecore.Globalization;

namespace RenderingsTest
{
    public class QuickerLanguageParser
    {
        private static Dictionary<string, bool> previouslyValid = new Dictionary<string, bool>();

        public static bool TryParse(string name, out Language result)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(name, "name");
            result = null;
            // !LanguageManager.IsValidLanguageName(name) was the issue here - replaced with my clumsy method
            if (name.Equals("__Standard Values", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("__language", StringComparison.OrdinalIgnoreCase) ||
                !IsValidLanguageName(name))
            {
                return false;
            }

            if (LanguageManager.LanguageRegistered(name) || LanguageManager.RegisterLanguage(name))
            {
                // can't do this properly
                result = Language.Parse(name);
                return true;
            }
            // or this....
            result = Language.Parse(name);
            return true;
        }

        private static bool IsValidLanguageName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }

            string lowerName = name.ToLowerInvariant();
            if (!previouslyValid.ContainsKey(lowerName))
            {
                previouslyValid.Add(lowerName, LanguageManager.IsValidLanguageName(name));
                
            }

            return previouslyValid[lowerName];
        }
    }
}
