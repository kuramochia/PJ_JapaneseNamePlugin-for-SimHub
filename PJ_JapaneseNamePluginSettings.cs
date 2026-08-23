using System;
using System.Collections.Generic;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class PJ_JapaneseNamePluginSettings
    {
        private string _jsonUrl = "https://gist.githubusercontent.com/kuramochia/0ccf486b022a9983c79c5c263646c7c9/raw/PJ_JapaneseNamePluginData.json";
        public string JsonUrl
        {
            get
            {
                // URL fix for old version of the plugin, to avoid breaking existing users
                if (_jsonUrl == "https://gist.githubusercontent.com/kuramochia/0ccf486b022a9983c79c5c263646c7c9/raw/c91ed83af694fdac03f6b12be58c50992063deee/PJ_JapaneseNamePluginData.json")
                {
                    _jsonUrl = "https://gist.githubusercontent.com/kuramochia/0ccf486b022a9983c79c5c263646c7c9/raw/PJ_JapaneseNamePluginData.json";
                }
                return _jsonUrl;
            }
            set { _jsonUrl = value; }
        }
        public string Etag { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;

        public Dictionary<string, string> Cities { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Companies { get; set; } = new Dictionary<string, string>();

        public bool IsExperimentalEnabled { get; set; } = false;
    }

    public class JapaneseTranslatedSettings
    {
        public Dictionary<string, string> TranslatedCities { get; set; } = new Dictionary<string, string>();
    }
}