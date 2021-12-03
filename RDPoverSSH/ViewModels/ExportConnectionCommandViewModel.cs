using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class ExportConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override string Description => Resources.ExportCommandDescription;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(ExportConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.UpArrow;

        #endregion

        #region Command

        private async void ExportConnectionItem(object param)
        {
            if (param is ConnectionViewModel connectionViewModel)
            {
                await MessageBoxHelper.Show("TODO", "TODO", MessageBoxButton.OK);
            }
        }

        #endregion
    }
}
