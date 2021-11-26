using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Controls;

namespace RDPoverSSH.ViewModels
{
    public class DuplicateConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(DeleteConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph { get; set; } = "\xE8C8";

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
