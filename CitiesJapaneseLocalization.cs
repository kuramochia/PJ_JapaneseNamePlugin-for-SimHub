using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    public class CitiesJapaneseLocalization : IPluginAction
    {

        private const string JobJapaneseCitySourcePropertyName = "Job.Ja.CitySource";
        private const string JobJapaneseCityDestinationPropertyName = "Job.Ja.CityDestination";

        private readonly PJ_JapaneseNamePlugin _plugin;

        private HttpClient _httpclient = new HttpClient();

        public CitiesJapaneseLocalization(PJ_JapaneseNamePlugin plugin)
        {
            _plugin = plugin;
        }

        public async Task InitAsync(CancellationToken cancellationToken)
        {

            _plugin.PluginManager.AddProperty(JobJapaneseCitySourcePropertyName, _plugin.GetType(), "", "日本語に翻訳された配送元都市名");
            _plugin.PluginManager.AddProperty(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), "", "日本語に翻訳された配送先都市名");
            await UpdateAsync(cancellationToken);
        }

        public Task UpdateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void DataUpdate()
        {
            if (_plugin.PluginManager.GameName == "ETS2" || _plugin.PluginManager.GameName == "ATS")
            {
                // 標準の都市名
                string defaultCitySourceName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CitySource")?.ToString();
                string defaultCityDestinationName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CityDestination")?.ToString();

                // 都市名の翻訳タスク
                var translatedCitySourceTask = TryGetTranslatedCityNameAsync(defaultCitySourceName);
                var translatedCityDestinationTask = TryGetTranslatedCityNameAsync(defaultCityDestinationName);

                // 翻訳された都市名を設定
                _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), translatedCitySourceTask.Result.translatedCityName);
                _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), translatedCityDestinationTask.Result.translatedCityName);
            }
        }


        private async Task<(bool found, string translatedCityName)> TryGetTranslatedCityNameAsync(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
            {
                return (false, string.Empty);
            }
            if (_plugin.TranslatedSettings.TranslatedCities.TryGetValue(cityName, out var translated))
            {
                return (true, translated);
            }

            try
            {
                // Google Translate の非公開 API を使用して都市名を日本語に翻訳する
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ja&dt=t&q={System.Net.WebUtility.UrlEncode(cityName)}";
                // HttpClient は頻繁に呼び出されるので、クラスのフィールドとして保持して再利用
                var response = await _httpclient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return (false, cityName);
                }

                var body = await response.Content.ReadAsStringAsync();

                // 取得成功。INFO としてログ出力しておく
                SimHub.Logging.Current.Info($"PJ_JapaneseNamePlugin Translate Success. {cityName} -> {body}");

                var arr = Newtonsoft.Json.Linq.JArray.Parse(body);
                // こんな Json が返却される
                // [[["アテネ","Αθήνα",null,null,11,null,null,[[]],[[["466914b2b9b759682681a550c00b67dd","en_ja_2023q1.md"]]]]],null,"el",null,null,null,1,[],[["el"],null,[1],["el"]]]
                var translatedText = arr?[0]?[0]?[0]?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(translatedText))
                {
                    // 翻訳結果を設定として保存する
                    _plugin.TranslatedSettings.TranslatedCities[cityName] = translatedText;
                    SimHub.Logging.Current.Info($"PJ_JapaneseNamePlugin Translate City Added. {cityName} -> {translatedText}");
                    return (true, translatedText);
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error("PJ_JapaneseNamePlugin Translate Error.", ex);
            }

            return (false, cityName);
        }

    }
}
