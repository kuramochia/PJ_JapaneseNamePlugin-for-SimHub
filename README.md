# PJ Japanese Name Plugin for SimHub

## これは何？
ProjectJapan で addon_jp_company を入れたときに、Simhub で表示する地名と会社名を日本語で表示するためのプラグインです。

以前は この [ProjectJapan で addon_jp_company を入れたときに、Simhub の地名と会社名を日本語で表示したい](https://gist.github.com/kuramochia/7337149e3fde89f07e34696a458ca8b8) で JavaScript Extension を配布していましたが、自動更新させたかったのでプラグインを作成しました。


## インストール方法
+ Releases から DLL (`kuramochia.PJ_JapaneseNamePlugin.dll`) を取得
+ SimHub インストール先 (通常は `C:\Program Files (x86)\SimHub`) に DLL をコピー
+ SimHub を再起動すると、プラグインをロードするダイアログが表示されます


## 使い方
下記のデータ(プロパティ)が追加されます。

+ 各プロパティは、Project Japan マップの場合（＝都市名が変換可能）はそれぞれ日本語の情報が取得できます。
+ バニラマップ等、Project Japan マップ以外の場合は、標準の情報に差し替えます。
  + 例えば配送元企業名の場合は、`GameRawData.JobValues.CompanySource` が返却されます。
  + Project Japan と別のマップでダッシュボードを切り替えなくても利用可能な設計です。

ダッシュボードで NCalc や JavaScript を使って、下記のデータを利用してください。

| プロパティ名 | 用途 | PJ 以外の場合の返却値 | 補足 |
| :--- | :--- | :--- | :--- |
| `PJ_JapaneseNamePlugin.Job.CitySource`  | 日本語の配送元都市名 | `GameRawData.JobValues.CitySource` | |
| `PJ_JapaneseNamePlugin.Job.CityDestination`  | 日本語の配送先都市名 | `GameRawData.JobValues.CityDestination` | |
| `PJ_JapaneseNamePlugin.Job.CitySource.NoCompletion`  | 日本語の配送元都市名(補完無し) | `''` | PJ の都市名が見つからない場合は empty を返却、条件分岐用 |
| `PJ_JapaneseNamePlugin.Job.CityDestination.NoCompletion`  | 日本語の配送先都市名(補完無し) | `''` | PJ の都市名が見つからない場合は empty を返却、条件分岐用 |
| `PJ_JapaneseNamePlugin.Job.CompanySource`  | 日本語の配送元企業名 | `GameRawData.JobValues.CompanySource` | |
| `PJ_JapaneseNamePlugin.Job.CompanyDestination`  | 日本語の配送先企業名 | `GameRawData.JobValues.CompanyDestination` | |

## Project Japan マップの日本語データについて
この [gist](https://gist.github.com/kuramochia/0ccf486b022a9983c79c5c263646c7c9/) の JSON データをプラグインが取りに行きます。
必要に応じて、プラグインの設定画面から URL を変更してください。


## Project Japan マップの日本語データ変換に問題があった場合
都市名は `GameRawData.JobValues.CitySourceId` または `GameRawData.JobValues.CityDestinationId`を、企業名は `GameRawData.JobValues.CompanySourceId` または `GameRawData.JobValues.CompanyDestinationId` を添えて教えてください。


### 使用例
#### 配送元都市名の使用例
```javascript
var city = $prop('PJ_JapaneseNamePlugin.Job.CitySource.NoCompletion')
if (city === '')
{
	var citySource = $prop('GameRawData.JobValues.CitySource');
	if(citySource === null || citySource === '') return '';
	city = $prop('PJ_JapaneseNamePlugin.Job.Ja.CitySource') + ' (' + citySource + ')';
}
return city;
```