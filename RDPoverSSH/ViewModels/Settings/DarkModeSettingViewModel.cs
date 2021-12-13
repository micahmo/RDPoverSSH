using System;
using System.ComponentModel;
using System.Windows.Threading;
using ModernWpf;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels.Settings
{
    public class DarkModeSetting : SettingViewModel<bool>
    {
        public DarkModeSetting() : base(new Guid("7A6271C3-E244-4A2E-AB51-68B046C221D0"), Resources.ApplicationSettingsCategory, Resources.DarkModeSettingName, defaultValue: false)
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
    }
}
