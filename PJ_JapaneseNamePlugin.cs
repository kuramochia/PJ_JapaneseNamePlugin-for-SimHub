using GameReaderCommon;
using SimHub.Plugins;
using System.Threading;
using System.Windows.Media;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    [PluginName("PJ Japanese Name Plugin")]
    [PluginDescription("ProjectJapan で日本語の地名、会社名を表示")]
    [PluginAuthor("kuramochia")]
    public class PJ_JapaneseNamePlugin : IPlugin, IDataPlugin, IWPFSettingsV2
    {
        public PJ_JapaneseNamePluginSettings Settings;
        public CancellationTokenSource EndTokenSource = new CancellationTokenSource();

        public PJLocalization Localization { get; private set; }

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "PJ Japanese Name Plugin";


        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            EndTokenSource.Cancel();
            EndTokenSource.Dispose();
            Save();
        }

        public void Save()
        {
            this.SaveCommonSettings("Settings", Settings);
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
            => new PJ_JapaneseNamePluginSettingsControl(this);

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            // Load settings
            Settings = this.ReadCommonSettings<PJ_JapaneseNamePluginSettings>("Settings", () => new PJ_JapaneseNamePluginSettings());

            Localization = new PJLocalization(this);
            Localization.InitAsync(EndTokenSource.Token).ConfigureAwait(false);
        }

        void IDataPlugin.DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            // Update Data
            if (data.OldData != null)
            {
                if (data.GameName == "ETS2")
                {
                    Localization.DataUpdate();
                }
            }
        }
    }
}