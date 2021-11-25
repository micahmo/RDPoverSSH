using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using RDPoverSSH.Controls;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels
{
    public class DeleteConnectionItemViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        //public override string Name => "Delete";
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(DeleteConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph { get; set; } = "\xE74D";

        #endregion

        #region Command

        private async void DeleteConnectionItem(object param)
        {
            if (param is ConnectionViewModel connectionViewModel)
            {
                var res = await MessageBoxHelper.Show($"Are you sure you want to delete connection '{connectionViewModel.Model.Name}'?", "Confirm", MessageBoxButton.YesNo);
                if (res == ContentDialogResult.Primary)
                {
                    RootModel.Instance.Connections.Remove(connectionViewModel.Model);
                    DatabaseEngine.ConnectionCollection.Delete(connectionViewModel.Model.ObjectId);
                }
            }
        }

        #endregion
    }
}
