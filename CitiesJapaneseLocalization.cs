using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            _httpclient.DefaultRequestHeaders.UserAgent.Clear();
            _httpclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/151.0.0.0 Safari/537.36 Edg/151.0.0.0");
        }

        public async Task InitAsync(CancellationToken cancellationToken)
        {

            _plugin.PluginManager.AddProperty(JobJapaneseCitySourcePropertyName, _plugin.GetType(), "", "日本語に翻訳された配送元都市名");
            _plugin.PluginManager.AddProperty(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), "", "日本語に翻訳された配送先都市名");
            await UpdateAsync(cancellationToken);
        }

        public Task UpdateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        private Dictionary<string, Task<TranslationResult>> _translationTasks = new Dictionary<string, Task<TranslationResult>>();

        public void DataUpdate()
        {
            if (_plugin.PluginManager.GameName == "ETS2" || _plugin.PluginManager.GameName == "ATS")
            {
                // 標準の都市名
                string defaultCitySourceName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CitySource")?.ToString();
                string defaultCityDestinationName = _plugin.PluginManager.GetPropertyValue("GameRawData.JobValues.CityDestination")?.ToString();

                // source の都市名処理
                if (string.IsNullOrEmpty(defaultCitySourceName))
                {
                    // 標準の都市名が null の場合は、翻訳処理を行わないで、そのまま返却する
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), defaultCitySourceName);
                }
                else if (_plugin.TranslatedSettings.TranslatedCities.TryGetValue(defaultCitySourceName, out var translated))
                {
                    // 標準の都市名が翻訳済みの場合は、翻訳結果を設定する
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), translated);
                }
                else if (_translationTasks.TryGetValue(defaultCitySourceName, out var sourceTask))
                {
                    // 翻訳タスクが既に存在する場合
                    if (sourceTask.IsCompleted)
                    {
                        // タスクが完了している場合は、翻訳結果を設定する
                        _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), sourceTask.Result.Name);
                        // 翻訳失敗した場合は、オンメモリでタスクが残り続けるので、翻訳結果が見つかった場合のみタスクを削除する
                        if (sourceTask.Result.Found)
                        {
                            // 翻訳結果が見つかった場合は、タスクを削除する
                            _translationTasks.Remove(defaultCitySourceName);
                        }
                    }
                    else
                    {
                        // タスクが完了していない場合は、元の都市名を設定する
                        _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), defaultCitySourceName);
                    }
                }
                else
                {
                    var newSourceTask = TryGetTranslatedCityNameAsync(defaultCitySourceName);
                    _translationTasks[defaultCitySourceName] = newSourceTask;
                    // すぐに翻訳結果が得られないため、元の都市名を設定
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCitySourcePropertyName, _plugin.GetType(), defaultCitySourceName);
                }

                // destination の都市名処理
                if (string.IsNullOrEmpty(defaultCityDestinationName))
                {
                    // 標準の都市名が null の場合は、翻訳処理を行わないで、そのまま返却する
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), defaultCityDestinationName);
                }
                else if (_plugin.TranslatedSettings.TranslatedCities.TryGetValue(defaultCityDestinationName, out var translatedDest))
                {
                    // 標準の都市名が翻訳済みの場合は、翻訳結果を設定する
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), translatedDest);
                }
                else if (_translationTasks.TryGetValue(defaultCityDestinationName, out var destTask))
                {
                    // 翻訳タスクが既に存在する場合
                    if (destTask.IsCompleted)
                    {
                        // タスクが完了している場合は、翻訳結果を設定する
                        _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), destTask.Result.Name);
                        // 翻訳失敗した場合は、オンメモリでタスクが残り続けるので、翻訳結果が見つかった場合のみタスクを削除する
                        if (destTask.Result.Found)
                        {
                            // 翻訳結果が見つかった場合は、タスクを削除する
                            _translationTasks.Remove(defaultCityDestinationName);
                        }
                    }
                    else
                    {
                        // タスクが完了していない場合は、元の都市名を設定する
                        _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), defaultCityDestinationName);
                    }
                }
                else
                {
                    var newDestTask = TryGetTranslatedCityNameAsync(defaultCityDestinationName);
                    _translationTasks[defaultCityDestinationName] = newDestTask;
                    // すぐに翻訳結果が得られないため、元の都市名を設定
                    _plugin.PluginManager.SetPropertyValue(JobJapaneseCityDestinationPropertyName, _plugin.GetType(), defaultCityDestinationName);
                }
            } 
        }

        private async Task<TranslationResult> TryGetTranslatedCityNameAsync(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
            {
                return new TranslationResult { Found = false, Name = string.Empty };
            }
            try
            {
                // Google Translate の非公開 API を使用して都市名を日本語に翻訳する
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl=ja&dt=t&q={System.Net.WebUtility.UrlEncode(cityName)}";
                // HttpClient は頻繁に呼び出されるので、クラスのフィールドとして保持して再利用
                var response = await _httpclient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    SimHub.Logging.Current.Error($"PJ_JapaneseNamePlugin Translate Error.{response.StatusCode}:{response.ReasonPhrase}");
                    return new TranslationResult { Found = false, Name = cityName };
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
                    return new TranslationResult { Found = true, Name = translatedText };
                }
            }
            catch (Exception ex)
            {
                SimHub.Logging.Current.Error("PJ_JapaneseNamePlugin Translate Error.", ex);
            }

            return new TranslationResult { Found = false, Name = cityName };
        }

        private struct TranslationResult
        {
            public bool Found { get; set; }
            public string Name { get; set; }
        }
    }
}
