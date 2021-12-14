using System.Diagnostics;
using RDPoverSSH.Common;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels.Settings
{
    public class SshSettingModel : MyObservableObject, ISettingModel
    {
        #region Constructor

        public SshSettingModel(string settingName, string defaultValue)
        {
            _settingName = settingName;
            _defaultValue = defaultValue;
        }

        #endregion

        #region ISettingModel members

        public string Value
        {
            get => _value ??= SshUtils.GetConfigValue(_settingName, includeCommented: true, defaultValue: _defaultValue);
            set => _value = value;
        }
        private string _value;

        public void Save()
        {
            var setSshdSettingProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = App.GetApplicationFilePath(),
                    Arguments = $"sshsetting {_settingName} {Value}",
                    // Run as admin
                    Verb = "runas",
                    // Required for runas
                    UseShellExecute = true
                }
            };

            try
            {
                setSshdSettingProcess.Start();
                setSshdSettingProcess.WaitForExit();
            }
            catch
            {
                // Swallow
            }

            // Force re-evaluation from file
            _value = null;
            OnPropertyChanged(nameof(Value));
        }

        #endregion

        #region Private fields

        private readonly string _settingName;
        private readonly string _defaultValue;

        #endregion
    }
}
