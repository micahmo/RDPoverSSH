using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RDPoverSSH.Common;
using RDPoverSSH.Properties;
using RDPoverSSH.ViewModels.Settings;

namespace RDPoverSSH.ViewModels
{
    public class SettingsDialogViewModel
    {
        public List<IGrouping<string, SettingViewModelBase>> Settings => _settings ??= new Func<List<IGrouping<string, SettingViewModelBase>>>(() =>
        {
            var settings = new List<SettingViewModelBase>
            {
                GlobalSettings.DarkModeSetting
            };

            if (File.Exists(Values.SshdConfigFile))
            {
                settings.Add(new SettingViewModel<bool, SshSettingModel>(new SshSettingModel("PasswordAuthentication", false.ToString()), Resources.SshServerSettings, Resources.AllowPasswordAuthentication, defaultValue: true, description: Resources.AllowPasswordAuthenticationSettingDescription));
            }

            return settings.GroupBy(setting => setting.Category).ToList();
        })();
        private List<IGrouping<string, SettingViewModelBase>> _settings;
    }
}
