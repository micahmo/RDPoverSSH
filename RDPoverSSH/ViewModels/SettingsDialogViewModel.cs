using System.Collections.Generic;
using System.Linq;
using RDPoverSSH.ViewModels.Settings;

namespace RDPoverSSH.ViewModels
{
    public class SettingsDialogViewModel
    {
        public List<IGrouping<string, SettingViewModelBase>> Settings { get; } = new List<SettingViewModelBase>
        {
            GlobalSettings.DarkModeSetting
        }.GroupBy(setting => setting.Category).ToList();
    }
}
