using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using ModernWpf;
using RDPoverSSH.DataStore;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels.Settings
{
    public class DarkModeSetting : SettingViewModel<bool, PersistentSettingModel>
    {
        public DarkModeSetting() : base(
            DatabaseEngine.GetCollection<PersistentSettingModel>().Find(setting => setting.Guid == DarkModeSettingGuid).FirstOrDefault() ?? new PersistentSettingModel {Guid = DarkModeSettingGuid},
            Resources.ApplicationSettingsCategory, Resources.DarkModeSettingName, defaultValue: false)
        {
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Change the theme
            Dispatcher.CurrentDispatcher.Invoke(() => ThemeManager.Current.ApplicationTheme = Value ? ApplicationTheme.Dark : ApplicationTheme.Light);

            ApplicationThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ApplicationThemeChanged;

        private static readonly Guid DarkModeSettingGuid = new Guid("7A6271C3-E244-4A2E-AB51-68B046C221D0");
    }
}
