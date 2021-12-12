using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using RDPoverSSH.Properties;
using RDPoverSSH.Utilities;

namespace RDPoverSSH.ViewModels
{
    public class DeleteConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override string Description => Resources.DeleteConnectionCommanDescription;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(DeleteConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Trash;

        #endregion

        #region Command

        private async void DeleteConnectionItem(object param)
        {
            if (param is ConnectionViewModel connectionViewModel)
            {
                string connectionNameFormat = string.IsNullOrWhiteSpace(connectionViewModel.Model.Name)
                    ? string.Empty
                    : $" '{connectionViewModel.Model.Name}'";

                var res = await MessageBoxHelper.Show(string.Format(Resources.ConfirmDeleteConnection, connectionNameFormat), Resources.Confirm, MessageBoxButton.YesNo);
                if (res == ContentDialogResult.Primary)
                {
                    RootModel.Instance.Connections.Remove(connectionViewModel.Model);
                    DatabaseEngine.GetCollection<ConnectionModel>().Delete(connectionViewModel.Model.ObjectId);

                    // Delete associated RDP Profiles
                    foreach (FileInfo fileInfo in new DirectoryInfo(Values.ApplicationDataPath).GetFiles(RdpUtils.RdpConnectionFilesWildcard(connectionViewModel.Model.ObjectId)))
                    {
                        File.Delete(fileInfo.FullName);
                    }
                }
            }
        }

        #endregion
    }
}
