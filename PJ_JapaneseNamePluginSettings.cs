using System;
using System.Collections.Generic;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class PJ_JapaneseNamePluginSettings
    {
        public string JsonUrl { get; set; } = "https://gist.githubusercontent.com/kuramochia/0ccf486b022a9983c79c5c263646c7c9/raw/c91ed83af694fdac03f6b12be58c50992063deee/PJ_JapaneseNamePluginData.json";
        public string Etag { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;

        public Dictionary<string, string> Cities { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Companies { get; set; } = new Dictionary<string, string>();

    }

    public class JapaneseTranslatedSettings
    {
        public Dictionary<string, string> TranslatedCities { get; set; } = new Dictionary<string, string>();
    }
}