using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// Command that opens settings
    /// </summary>
    public class SettingsCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name => "Settings";

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(SettingsCommand);
        private RelayCommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => "\xE713";

        #endregion

        #region Commands

        private void SettingsCommand() { }

        #endregion
    }
}
