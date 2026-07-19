using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    public class PJLocalization
    {
        private const string JobCitySourcePropertyName = "Job.CitySource";
        private const string JobCityDestinationPropertyName = "Job.CityDestination";
        private const string JobCompanySourcePropertyName = "Job.CompanySource";
        private const string JobCompanyDestinationPropertyName = "Job.CompanyDestination";

        private readonly PJ_JapaneseNamePlugin _plugin;

        private HttpClient _httpclient = new HttpClient();

        public PJLocalization(PJ_JapaneseNamePlugin plugin)
        {
            _plugin = plugin;
        }

        public async Task InitAsync(CancellationToken cancellationToken)
        {
            _plugin.PluginManager.AddProperty(JobCitySourcePropertyName, _plugin.GetType(), "", "PJ Map では日本語の配送元都市名、それ以外は通常の配送元都市名");
            _plugin.PluginManager.AddProperty(JobCityDestinationPropertyName, _plugin.GetType(), "", "PJ Map では日本語の配送先都市名、それ以外は通常の配送先都市名");
            _plugin.PluginManager.AddProperty(JobCompanySourcePropertyName, _plugin.GetType(), "", "PJ Map では日本語の配送元企業名、それ以外は通常の配送元企業名");
            _plugin.PluginManager.AddProperty(JobCompanyDestinationPropertyName, _plugin.GetType(), "", "PJ Map では日本語の配送先企業名、それ以外は通常の配送先企業名");
            await UpdateAsync(cancellationToken);
        }

        public async Task UpdateAsync(CancellationToken cancellationToken = default) => await UpdateAsync(false, cancellationToken);
        public async Task UpdateAsync(bool forceUpdate, CancellationToken cancellationToken = default)
        {
            // ローカライゼーション JSON データを取得する
            var jsonUrl = _plugin.Settings.JsonUrl;
            if (string.IsNullOrEmpty(jsonUrl))
            {
                return;
            }

            // forceUpdate が true の場合は ETag を無視して更新する
            var checkEtag = !string.IsNullOrEmpty(_plugin.Settings.Etag);

            _httpclient.DefaultRequestHeaders.IfNoneMatch.Clear();
            // ETag で更新確認
            if (checkEtag && !forceUpdate)
            {
                _httpclient.DefaultRequestHeaders.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue($"\"{_plugin.Settings.Etag}\""));
            }

            try
            {
                var response = await _httpclient.GetAsync(jsonUrl, cancellationToken);
                if (!forceUpdate && checkEtag && response.StatusCode == System.Net.HttpStatusCode.NotModified)
                {
                    _plugin.Settings.LastUpdate = System.DateTime.Now;
                    // ETag が一致している場合は更新不要
                    return;
                }
                else if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<PJ_JapaneseNamePluginSettings>(json);
                    // データを更新
                    _plugin.Settings.Cities = data.Cities;
                    _plugin.Settings.Companies = data.Companies;
                    _plugin.Settings.Etag = response.Headers.ETag?.Tag?.Trim('"') ?? string.Empty;
                    _plugin.Settings.LastUpdate = System.DateTime.Now;
                }
                else
                {
                    SimHub.Logging.Current.Error($"PJ_JapaneseNamePlugin Update Json Error. \n {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error("PJ_JapaneseNamePlugin Update Json Error.", ex);
            }
        }

        internal void DataUpdate()
        {
            if (_plugin.PluginManager.GameName == "ETS2")
            {
                // JobValues から都市名と会社名の ID を取得
                string citySourceId = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CitySourceId")?.ToString();
                string cityDestinationId = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CityDestinationId")?.ToString();
                string companySourceId = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CompanySourceId")?.ToString();
                string companyDestinationId = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CompanyDestinationId")?.ToString();

                // ローカライズされた名前を取得、取得できない場合は string.Empty にする
                string citySourceName = string.Empty;
                string cityDestinationName = string.Empty;
                string companySourceName = string.Empty;
                string companyDestinationName = string.Empty;

                // 都市名を取得する
                _plugin.Settings.Cities?.TryGetValue(citySourceId, out citySourceName);
                _plugin.Settings.Cities?.TryGetValue(cityDestinationId, out cityDestinationName);

                var isPJMap = !string.IsNullOrEmpty(citySourceName) || !string.IsNullOrEmpty(cityDestinationName);

                // 会社名は、都市名が取得できている場合にのみ取得する（PJ 以外のマップの場合は、標準の名前を使用）
                if (!isPJMap)
                {
                    // 標準の会社名を使用する
                    companySourceName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CompanySource")?.ToString();
                    companyDestinationName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CompanyDestination")?.ToString();
                    // 標準の都市名を使用する
                    citySourceName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CitySource")?.ToString();
                    cityDestinationName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CityDestination")?.ToString();
                }
                else
                {
                    _plugin.Settings.Companies?.TryGetValue(companySourceId, out companySourceName);
                    _plugin.Settings.Companies?.TryGetValue(companyDestinationId, out companyDestinationName);
                }

                _plugin.PluginManager.SetPropertyValue(JobCitySourcePropertyName, _plugin.GetType(), citySourceName);
                _plugin.PluginManager.SetPropertyValue(JobCityDestinationPropertyName, _plugin.GetType(), cityDestinationName);
                _plugin.PluginManager.SetPropertyValue(JobCompanySourcePropertyName, _plugin.GetType(), companySourceName);
                _plugin.PluginManager.SetPropertyValue(JobCompanyDestinationPropertyName, _plugin.GetType(), companyDestinationName);
            }
        }
    }
}
