using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace RDPoverSSH.ViewModels
{
    /// <summary>
    /// Command that creates a new connection
    /// </summary>
    public class NewConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name => "New Connection";

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(NewConnectionCommand);
        private RelayCommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => "\xECC8";

        #endregion

        #region Commands

        private void NewConnectionCommand()
        {
            MessageBox.Show("yo");
        }

        #endregion
    }
}
