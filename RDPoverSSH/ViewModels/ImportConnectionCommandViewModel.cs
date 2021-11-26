using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// Command that opens settings
    /// </summary>
    public class ImportConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name => Resources.ImportConnection;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(ImportCommand);
        private RelayCommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => "\xE896";

        #endregion

        #region Commands

        private void ImportCommand() { }

        #endregion
    }
}
