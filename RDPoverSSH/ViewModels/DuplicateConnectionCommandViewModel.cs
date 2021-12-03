using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class DuplicateConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override string Description => Resources.DuplicateConnection;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(DeleteConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Copy;

        #endregion

        #region Command

        private async void DeleteConnectionItem(object param)
        {
            if (param is ConnectionViewModel connectionViewModel)
            {
                await MessageBoxHelper.Show("TODO", "TODO", MessageBoxButton.OK);
            }
        }

        #endregion
    }
}
