using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using RDPoverSSH.Common;
using RDPoverSSH.Controls;
using RDPoverSSH.Properties;
using RDPoverSSH.Utilities;

namespace RDPoverSSH.ViewModels
{
    public class RdpConnectionProfileCommandViewModel : CommandViewModelBase
    {
        #region Constructor

        public RdpConnectionProfileCommandViewModel(string name, string filename, ConnectionViewModel connection)
        {
            Name = name;
            _filename = filename;
            _connection = connection;
        }

        #endregion

        #region CommandViewModelBase members

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand(LaunchRdpProfile);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Connect;

        /// <inheritdoc/>
        public override string Description => string.Format(Resources.RdpConnectionProfileCommandDescription, Name);

        /// <inheritdoc/>
        public override List<CommandViewModelBase> SubCommands => _subCommands ??= new List<CommandViewModelBase>
        {
            new GenericCommandViewModel(Resources.Edit, new RelayCommand(EditRdpProfile), Icons.Edit),
            new GenericCommandViewModel(Resources.Rename, new RelayCommand(RenameRdpProfile), Icons.Rename),
            new GenericCommandViewModel(Resources.Delete, new RelayCommand(DeleteRdpProfile), Icons.Trash)
        };
        private List<CommandViewModelBase> _subCommands;

        #endregion

        #region Public events

        /// <summary>
        /// Fires when something is changed related to the connection profile
        /// </summary>
        public event EventHandler RdpConnectionProfileModified;

        #endregion

        #region Private methods

        private void LaunchRdpProfile()
        {
            RdpUtils.FixupRdpProfileHost(_filename, Values.MappedAddress(_connection.Model.LocalTunnelPort));
            Process.Start(RdpUtils.MstscExecutable, $"\"{_filename}\"");
        }

        private void EditRdpProfile()
        {
            RdpUtils.FixupRdpProfileHost(_filename, Values.MappedAddress(_connection.Model.LocalTunnelPort));
            Process.Start(RdpUtils.MstscExecutable, $"/edit \"{_filename}\"");
            RdpConnectionProfileModified?.Invoke(this, EventArgs.Empty);
        }

        private async void RenameRdpProfile()
        {
            string name = default;

            while (string.IsNullOrWhiteSpace(name))
            {
                var res = await MessageBoxHelper.ShowInputBox(Resources.RdpProfileNamePrompt, Resources.Name, initialInput: RdpUtils.GetRdpProfileNameFromFileName(_filename));
                name = RdpUtils.SanitizeFilename(res.Text);

                if (res.Result != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            string newRdpConnectionFilePath = RdpUtils.ChangeRdpProfileName(_filename, name);
            File.Move(_filename, newRdpConnectionFilePath);

            RdpConnectionProfileModified?.Invoke(this, EventArgs.Empty);
        }

        private void DeleteRdpProfile()
        {
            File.Delete(_filename);
            RdpConnectionProfileModified?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private fields

        private readonly string _filename;
        private readonly ConnectionViewModel _connection;

        #endregion
    }
}
