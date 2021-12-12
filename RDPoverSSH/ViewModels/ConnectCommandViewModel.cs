using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.Properties;
using RDPoverSSH.Utilities;

namespace RDPoverSSH.ViewModels
{
    public class ConnectCommandViewModel : CommandViewModelBase
    {
        #region Constructor

        public ConnectCommandViewModel(ConnectionViewModel connection)
        {
            _connection = connection;

            _connection.Model.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName.Equals(nameof(connection.Model.ConnectionPort)))
                {
                    OnPropertyChanged(nameof(SubCommands));
                }
            };
        }

        #endregion

        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name => Resources.Connect;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(Connect);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Connect;

        /// <inheritdoc/>
        public override List<CommandViewModelBase> SubCommands => _connection.SelectedConnectionPort == PortViewModel.RdpPort
            ? new Func<List<CommandViewModelBase>>(() =>
            {
                List<CommandViewModelBase> rdpFiles = new List<CommandViewModelBase>();

                foreach (FileInfo fileInfo in new DirectoryInfo(Values.ApplicationDataPath).GetFiles(RdpUtils.RdpConnectionFilesWildcard(_connection.Model.ObjectId)))
                {
                    var rdpConnectionProfileCommandViewModel = new RdpConnectionProfileCommandViewModel(RdpUtils.GetRdpProfileNameFromFileName(fileInfo.Name), fileInfo.FullName, _connection);
                    rdpConnectionProfileCommandViewModel.RdpConnectionProfileModified += (_, __) =>
                    {
                        OnPropertyChanged(nameof(SubCommands));
                    };
                    rdpFiles.Add(rdpConnectionProfileCommandViewModel);
                }

                rdpFiles.Add(new GenericCommandViewModel(Resources.AddRdpProfile, new RelayCommand(AddRdpProfile), Icons.Add, hasSubCommandSeparator: rdpFiles.Any()));

                return rdpFiles;
            })()
            : Enumerable.Empty<CommandViewModelBase>().ToList();

        #endregion

        #region Private methods

        private void Connect()
        {
            if (_connection.SelectedConnectionPort == PortViewModel.RdpPort)
            {
                Process.Start(RdpUtils.MstscExecutable, $"/v:{Values.MappedAddress(_connection.Model.LocalTunnelPort)}");
            }
            else
            {
                // Open the mapped port in the browser (with the correct protocol).
                Process.Start("explorer.exe", $"http{(_connection.SelectedConnectionPort == PortViewModel.HttpsPort ? "s" : string.Empty)}://{Values.MappedAddress(_connection.Model.LocalTunnelPort)}");
            }
        }

        private async void AddRdpProfile()
        {
            // Get a name first
            string name = default;

            while (string.IsNullOrWhiteSpace(name))
            {
                var res = await MessageBoxHelper.ShowInputBox(Resources.RdpProfileNamePrompt, Resources.Name);
                name = RdpUtils.SanitizeFilename(res.Text);

                if (res.Result != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            string newRdpConnectionFilePath = RdpUtils.NewRdpConnectionFilePath(_connection.Model.ObjectId, name);

#pragma warning disable CS0642
            using (var _ = File.Create(newRdpConnectionFilePath));
#pragma warning restore CS0642
            
            await File.WriteAllTextAsync(newRdpConnectionFilePath, $"full address:s:{Values.MappedAddress(_connection.Model.LocalTunnelPort)}");

            Process.Start(RdpUtils.MstscExecutable, $"/edit \"{newRdpConnectionFilePath}\"");
        }

        #endregion

        #region Private fields

        private readonly ConnectionViewModel _connection;

        #endregion
    }
}
