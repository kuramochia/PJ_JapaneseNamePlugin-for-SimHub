using System;
using System.Windows.Controls;

namespace Kuramochia.PJ_JapaneseNamePlugin
{
    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class PJ_JapaneseNamePluginSettingsControl : UserControl
    {
        public PJ_JapaneseNamePlugin Plugin { get; }

        public PJ_JapaneseNamePluginSettingsControl()
        {
            InitializeComponent();
        }

        public PJ_JapaneseNamePluginSettingsControl(PJ_JapaneseNamePlugin plugin) : this()
        {
            this.Plugin = plugin;
            jsonUrlTextBox.Text = Plugin.Settings.JsonUrl;
            Update_lastUpdatedTextBlock();
        }

        private async void updateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await Plugin.Localization.UpdateAsync(true);
            Update_lastUpdatedTextBlock();
        }

        private void jsonUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Plugin.Settings.JsonUrl = jsonUrlTextBox.Text;
        }

        private void Update_lastUpdatedTextBlock()
        {
            if (Plugin.Settings.LastUpdate == DateTime.MinValue)
            {
                lastUpdatedTextBlock.Text = "無し";
            }
            else
            {
                lastUpdatedTextBlock.Text = $"最終更新: {Plugin.Settings.LastUpdate.ToString("yyyy-MM-dd HH:mm:ss")}";
            }
        }
    }
}