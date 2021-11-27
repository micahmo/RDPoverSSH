using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Controls;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class ConnectionViewModel : ObservableObject
    {
        #region Constructor

        public ConnectionViewModel(ConnectionModel model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;

            // TODO: Make this converter?
            if (Model.ConnectionPort != default)
            {
                if (DefaultConnectionPorts.FirstOrDefault(p => p.Value == Model.ConnectionPort) is { } selectedPortViewModel)
                {
                    SelectedConnectionPort = selectedPortViewModel;
                }
                else
                {
                    SelectedConnectionPort = PortViewModel.Custom;
                }
            }

            if (Model.TunnelPort != default)
            {
                if (DefaultTunnelPorts.FirstOrDefault(p => p.Value == Model.TunnelPort) is { } selectedPortViewModel)
                {
                    SelectedTunnelPort = selectedPortViewModel;
                }
                else
                {
                    SelectedTunnelPort = PortViewModel.Custom;
                }
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Any time the model changes, persist it
            DatabaseEngine.ConnectionCollection.Update(Model);

            if (e.PropertyName.Equals(nameof(Model.ConnectionDirection)))
            {
                ToggleConnectionDirectionCommand.IconGlyph = ConnectionDirectionGlyph;
                
                // Update both descriptions, no matter which direction changed
                ToggleConnectionDirectionCommand.Description = ConnectionDirectionDescription;
                ToggleTunnelDirectionCommand.Description = TunnelDirectionDescription;
            }
            else if (e.PropertyName.Equals(nameof(Model.TunnelDirection)))
            {
                ToggleTunnelDirectionCommand.IconGlyph = TunnelDirectionGlyph;

                // Update both descriptions, no matter which direction changed
                ToggleConnectionDirectionCommand.Description = ConnectionDirectionDescription;
                ToggleTunnelDirectionCommand.Description = TunnelDirectionDescription;
            }
        }

        #endregion

        #region Public properties

        public ConnectionModel Model { get; }

        public DeleteConnectionCommandViewModel DeleteConnectionCommand { get; } = new DeleteConnectionCommandViewModel();

        public DuplicateConnectionCommandViewModel DuplicateConnectionCommand { get; } = new DuplicateConnectionCommandViewModel();

        public ExportConnectionCommandViewModel ExportConnectionCommand { get; } = new ExportConnectionCommandViewModel();

        public GenericCommandViewModel ToggleConnectionDirectionCommand => _toggleConnectionDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleConnectionDirection), ConnectionDirectionGlyph, ConnectionDirectionDescription);
        private GenericCommandViewModel _toggleConnectionDirectionCommand;

        private string ConnectionDirectionGlyph => Model.ConnectionDirection switch
        {
            Direction.Outgoing => "\xF0AF",
            Direction.Incoming => "\xF0B0",
            _ => "\xF0AF"
        };

        private string ConnectionDirectionDescription => Model.ConnectionDirection switch
        {
            Direction.Outgoing => string.Format(Resources.OutgoingConnectionDescription, Model.ConnectionPort),
            Direction.Incoming => string.Format(Resources.IncomingConnectionDescription, Model.ConnectionPort),
            _ => default
        };

        public GenericCommandViewModel ToggleTunnelDirectionCommand => _toggleTunnelDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleTunnelDirection), TunnelDirectionGlyph, TunnelDirectionDescription);
        private GenericCommandViewModel _toggleTunnelDirectionCommand;

        private string TunnelDirectionGlyph => Model.TunnelDirection switch
        {
            Direction.Outgoing => "\xF0AF",
            Direction.Incoming => "\xF0B0",
            _ => "\xF0AF"
        };

        private string TunnelDirectionDescription => Model.TunnelDirection switch
        {
            Direction.Outgoing => Model.IsReverseTunnel ? Resources.OutgoingReverseTunnelDescription : Resources.OutgoingTunnelDescription,
            Direction.Incoming => Model.IsReverseTunnel ? Resources.IncomingReverseTunnelDescription : Resources.IncomingTunnelDescription,
            _ => default
        };

        public string MachineName => string.Format(Resources.LocalComputerName, Environment.MachineName);

        #region Connection ports

        public List<PortViewModel> DefaultConnectionPorts { get; } = new List<PortViewModel>
        {
            new PortViewModel {Value = 3389, Name = "RDP"},
            new PortViewModel {Value = 445, Name = "SMB"},
            PortViewModel.Custom
        };

        public PortViewModel SelectedConnectionPort
        {
            get => _selectedConnectionPort;
            set
            {
                SetProperty(ref _selectedConnectionPort, value);
                Model.ConnectionPort = IsConnectionPortCustom
                    ? Model.ConnectionPort // Don't change it
                    : _selectedConnectionPort.Value; // Change it to the predefined value
                OnPropertyChanged(nameof(IsConnectionPortCustom));
            }
        }
        private PortViewModel _selectedConnectionPort;

        public bool IsConnectionPortCustom => SelectedConnectionPort == PortViewModel.Custom;

        #endregion

        #region Tunnel ports

        public List<PortViewModel> DefaultTunnelPorts { get; } = new List<PortViewModel>
        {
            new PortViewModel {Value = 22, Name = "SSH"},
            new PortViewModel {Value = 80, Name = "HTTP"},
            new PortViewModel {Value = 443, Name = "HTTPS"},
            PortViewModel.Custom
        };

        public PortViewModel SelectedTunnelPort
        {
            get => _selectedTunnelPort;
            set
            {
                SetProperty(ref _selectedTunnelPort, value);
                Model.TunnelPort = IsTunnelPortCustom
                    ? Model.TunnelPort // Don't change it
                    : _selectedTunnelPort.Value; // Change it to the predefined value
                OnPropertyChanged(nameof(IsTunnelPortCustom));
            }
        }
        private PortViewModel _selectedTunnelPort;

        public bool IsTunnelPortCustom => SelectedTunnelPort == PortViewModel.Custom;

        #endregion

        public GenericCommandViewModel ConnectionCommand => _connectionCommand ??=
            new GenericCommandViewModel(Resources.Connect, new RelayCommand(delegate { }), string.Empty);
        private GenericCommandViewModel _connectionCommand;

        public GenericCommandViewModel TestTunnelCommand => _testTunnelCommand ??=
            new GenericCommandViewModel(Resources.Test, new RelayCommand(delegate { }), string.Empty);
        private GenericCommandViewModel _testTunnelCommand;

        public GenericCommandViewModel ServerKeysCommand => _serverKeysCommand ??= 
            new GenericCommandViewModel(string.Empty, new RelayCommand(ShowServerKeys), "\xE875", Resources.ShowSshServerKey);
        private GenericCommandViewModel _serverKeysCommand;

        #endregion

        #region Private methods

        private void ToggleConnectionDirection()
        {
            var connectionDirection = Model.ConnectionDirection;
            Model.ConnectionDirection = connectionDirection.Toggle();
        }

        private void ToggleTunnelDirection()
        {
            var tunnelDirection = Model.TunnelDirection;
            Model.TunnelDirection = tunnelDirection.Toggle();
        }

        private async void ShowServerKeys()
        {
            string privateKeyText;

            var getPrivateKeyProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-noprofile -Command \"Get-Content \"{_ourPrivateKeyFilePath}\"\"",
                    RedirectStandardOutput = true,
                    // Need admin privs to read the key file
                    Verb = "runas",
                    CreateNoWindow = true
                }
            };

            if (File.Exists(_ourPrivateKeyFilePath))
            {
                getPrivateKeyProcess.Start();
                getPrivateKeyProcess.WaitForExit();
                privateKeyText = await getPrivateKeyProcess.StandardOutput.ReadToEndAsync();
            }
            else
            {
                privateKeyText = Resources.SshServerKeyNotFound;
            }

            await MessageBoxHelper.ShowCopyableText(Resources.SshPrivateKeyDescription, Resources.SshServerKeyHeading, privateKeyText, monospace: true);
        }

        #endregion

        #region Private fields

        private static readonly string SshProgramDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        private readonly string _ourPrivateKeyFilePath = Path.Combine(SshProgramDataPath, "ssh", "ssh_rdp_over_ssh_key");

        #endregion
    }
}
