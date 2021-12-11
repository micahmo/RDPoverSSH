using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Arguments;
using RDPoverSSH.Common;
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
            PropertyChanged += ConnectionViewModel_PropertyChanged;

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

        private void ConnectionViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Status)))
            {
                UpdateTunnelStatusInfo();
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Any time the model changes, persist it
            DatabaseEngine.GetCollection<ConnectionModel>().Update(Model);

            if (e.PropertyName.Equals(nameof(Model.ConnectionDirection)))
            {
                ToggleConnectionDirectionCommand.IconGlyph = ConnectionDirectionGlyph;
                
                // Update both descriptions, no matter which direction changed
                ToggleConnectionDirectionCommand.Description = ConnectionDirectionDescription;
                ToggleTunnelDirectionCommand.Description = TunnelDirectionDescription;
            }
            
            if (e.PropertyName.Equals(nameof(Model.TunnelDirection)))
            {
                ToggleTunnelDirectionCommand.IconGlyph = TunnelDirectionGlyph;

                // Update both descriptions, no matter which direction changed
                ToggleConnectionDirectionCommand.Description = ConnectionDirectionDescription;
                ToggleTunnelDirectionCommand.Description = TunnelDirectionDescription;

                // Update server keys tooltip
                ServerKeysCommand.Description = ServerKeysDescription;

                UpdateTunnelStatusInfo();
            }

            if (e.PropertyName.Equals(nameof(Model.TunnelDirection))
                || e.PropertyName.Equals(nameof(Model.ConnectionDirection)))
            {
                Model.LocalTunnelPort = Model.TunnelDirection switch
                {
                    Direction.Outgoing => Model.IsReverseTunnel switch
                    {
                        true => 0,
                        false => NetworkUtils.GetFreeTcpPort()
                    },
                    Direction.Incoming => Model.IsReverseTunnel switch
                    {
                        true => NetworkUtils.GetFreeTcpPort(),
                        false => 0
                    },
                    _ => default
                };
            }

            if (e.PropertyName.Equals(nameof(Model.TunnelDirection))
                || e.PropertyName.Equals(nameof(Model.LocalTunnelPort))
                || e.PropertyName.Equals(nameof(Model.ConnectionPort)))
            {
                OnPropertyChanged(nameof(LocalTunnelPortDescription));
            }

            if (e.PropertyName.Equals(nameof(Model.TunnelDirection))
                || e.PropertyName.Equals(nameof(Model.TunnelEndpoint))
                || e.PropertyName.Equals(nameof(Model.TunnelPort))
                || e.PropertyName.Equals(nameof(Model.LocalTunnelPort))
                || e.PropertyName.Equals(nameof(Model.ConnectionPort)))
            {
                Status = TunnelStatus.Unknown;
                RemoteMachineName = Resources.RemoteComputer;
            }
        }

        private void UpdateTunnelStatusInfo()
        {
            var tunnelStatusInfo = TunnelStatusInfo;
            TunnelStatusButton.Description = tunnelStatusInfo.Description;
            TunnelStatusButton.IconGlyph = tunnelStatusInfo.Glyph;
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
            Direction.Outgoing => Icons.RightArrow,
            Direction.Incoming => Icons.LeftArrow,
            _ => default
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
            Direction.Outgoing => Icons.RightArrow,
            Direction.Incoming => Icons.LeftArrow,
            _ => default
        };

        private string TunnelDirectionDescription => Model.TunnelDirection switch
        {
            Direction.Outgoing => Model.IsReverseTunnel ? Resources.OutgoingReverseTunnelDescription : Resources.OutgoingTunnelDescription,
            Direction.Incoming => Model.IsReverseTunnel ? Resources.IncomingReverseTunnelDescription : Resources.IncomingTunnelDescription,
            _ => default
        };

        public string MachineName => string.Format(Resources.LocalComputerName, Environment.MachineName);

        public string RemoteMachineName
        {
            get => _remoteMachineName;
            set => SetProperty(ref _remoteMachineName, value);
        }
        private string _remoteMachineName = Resources.RemoteComputer;

        #region Connection ports

        public List<PortViewModel> DefaultConnectionPorts { get; } = new List<PortViewModel>
        {
            PortViewModel.RdpPort,
            PortViewModel.HttpPort,
            PortViewModel.HttpsPort,
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
            PortViewModel.SshPort,
            PortViewModel.HttpPort,
            PortViewModel.HttpsPort,
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

        public ConnectCommandViewModel ConnectCommand => _connectCommand ??= new ConnectCommandViewModel(this);
        private ConnectCommandViewModel _connectCommand;

        public GenericCommandViewModel TunnelStatusButton => _tunnelStatusButton ??= new Func<GenericCommandViewModel>(() =>
        {
            var tunnelStatusInfo = TunnelStatusInfo;
            return new GenericCommandViewModel(string.Empty, new RelayCommand(ShowLastError), tunnelStatusInfo.Glyph, tunnelStatusInfo.Description);
        })();
        private GenericCommandViewModel _tunnelStatusButton;

        public GenericCommandViewModel ServerKeysCommand => _serverKeysCommand ??= 
            new GenericCommandViewModel(string.Empty, new RelayCommand(HandleServerKeys), Icons.Lock, ServerKeysDescription);
        private GenericCommandViewModel _serverKeysCommand;

        private string ServerKeysDescription => Model.TunnelDirection switch
        {
            Direction.Incoming => Resources.ShowSshServerKey,
            Direction.Outgoing => Resources.AddSshServerKey,
            _ => default
        };

        public (string Description, string Glyph) TunnelStatusInfo
        {
            get
            {
                return Model.TunnelDirection switch
                {
                    Direction.Outgoing => Status switch
                    {
                        TunnelStatus.Unknown => (Resources.UnknownTunnelStatus, Icons.Question),
                        TunnelStatus.Disconnected => (Resources.DisconnectedTunnelStatus, Icons.X),
                        TunnelStatus.Connected => (Resources.ConnectedTunnelStatus, Icons.Check),
                        _ => default
                    },
                    Direction.Incoming => Status switch
                    {
                        TunnelStatus.Disconnected => (Resources.SshServerNotRunning, Icons.X),
                        TunnelStatus.Connected => Model.IsReverseTunnel 
                            ? (string.Format(Resources.SshServerReverseTunnelRunning, Model.LocalTunnelPort), Icons.Check) 
                            : (string.Format(Resources.SshServerRunning, SshUtils.GetConfigValue("Port", 23, includeCommented: true)), Icons.Check),
                        TunnelStatus.Unknown => (Resources.SshStateUnknown, Icons.Question),
                        TunnelStatus.Partial => (string.Format(Resources.SshServerRunningNoReverseTunnel, SshUtils.GetConfigValue("Port", 23, includeCommented: true), Model.LocalTunnelPort), Icons.Warning),
                        _ => default
                    },
                    _ => default
                };
            }
        }

        /// <summary>
        /// This is only a UI property
        /// </summary>
        public TunnelStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                LastStatusUpdateDateTime = DateTimeOffset.Now;
            }
        }

        private TunnelStatus _status;

        public DateTimeOffset LastStatusUpdateDateTime { get; private set; }

        public string LastError
        {
            get => _lastError;
            set => SetProperty(ref _lastError, value);
        }
        private string _lastError;

        public string LocalTunnelPortDescription => Model.TunnelDirection switch
        {
            Direction.Outgoing => string.Join(' ', string.Format(Resources.OutgoingTunnelLocalTunnelPortDescription, Model.LocalTunnelPort, Model.ConnectionPort), Resources.ChangeOutgoingLocalTunnelPortWarning),
            Direction.Incoming => string.Join(' ', string.Format(Resources.IncomingTunnelLocalTunnelPortDescription, Model.LocalTunnelPort, Model.ConnectionPort), Resources.ChangeIncomingLocalTunnelPortWarning),
            _ => default
        };

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

        private async void HandleServerKeys()
        {
            if (Model.TunnelDirection == Direction.Incoming)
            {
                await ShowServerKeys();
            }
            else if (Model.TunnelDirection == Direction.Outgoing)
            {
                await AcceptServerKeys();
            }
        }

        private async Task ShowServerKeys()
        {
            if (File.Exists(Values.OurPrivateKeyFilePath))
            {
                var getPrivateKeyProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = GetApplicationFilePath(),
                        Arguments = $"showmessage {ShowMessageArgument.SshServerPrivateKey}",
                        // Run as admin
                        Verb = "runas",
                        // Required for runas
                        UseShellExecute = true
                    }
                };

                try
                {
                    getPrivateKeyProcess.Start();
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode == 1223) // ERROR_CANCELLED
                    {
                        await MessageBoxHelper.Show(Resources.AdminRequiredForPrivateKey, Resources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
                        await MessageBoxHelper.Show(Resources.ErrorGettingPrivateKey, Resources.Error, MessageBoxButton.OK);
                    }
                }
            }
            else
            {
                await MessageBoxHelper.Show(Resources.SshServerKeyNotGenerated, Resources.NotFound, MessageBoxButton.OK);
            }
        }

        private async Task AcceptServerKeys()
        {
            DateTime previousClientServerKeyFileLastModifiedTime = new FileInfo(Values.ClientServerPrivateKeyFilePath(Model.ObjectId)).LastWriteTimeUtc;

            var savePrivateKeyProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetApplicationFilePath(),
                    Arguments = $"savekey {SaveKeyArgument.SshServerPrivateKey} {Model.ObjectId}",
                    // Run as admin
                    Verb = "runas",
                    // Required for runas
                    UseShellExecute = true
                },
                // For Exited event
                EnableRaisingEvents = true
            };

            try
            {
                savePrivateKeyProcess.Exited += (_, __) =>
                {
                    // The process has exited. Did the user update the key? If so, go into Unknown state.
                    DateTime newClientServerKeyFileLastModifiedTime = new FileInfo(Values.ClientServerPrivateKeyFilePath(Model.ObjectId)).LastWriteTimeUtc;
                    if (newClientServerKeyFileLastModifiedTime > previousClientServerKeyFileLastModifiedTime)
                    {
                        // Raise something that we know will put us in the unknown state so we can centralize the logic.
                        Model.RaisePropertyChanged(nameof(Model.TunnelPort));
                    }
                };

                savePrivateKeyProcess.Start();
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1223) // ERROR_CANCELLED
                {
                    await MessageBoxHelper.Show(Resources.AdminRequiredToSavePrivateKey, Resources.Error, MessageBoxButton.OK);
                }
                else
                {
                    await MessageBoxHelper.Show(Resources.ErrorSavingPrivateKey, Resources.Error, MessageBoxButton.OK);
                }
            }
        }

        private async void ShowLastError()
        {
            if (Status == TunnelStatus.Disconnected && Model.TunnelDirection == Direction.Outgoing)
            {
                await MessageBoxHelper.ShowCopyableText(string.Format(Resources.ErrorConnectingToTunnel, Model.TunnelEndpoint, Model.TunnelPort), Resources.ConnectionError, LastError);
            }
        }

        private static string GetApplicationFilePath()
        {
            string currentApplicationFilePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase ?? string.Empty).LocalPath;
            string currentApplicationFolderPath = Path.GetDirectoryName(currentApplicationFilePath) ?? string.Empty;
            if (Path.GetExtension(currentApplicationFilePath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                currentApplicationFilePath = Path.Combine(currentApplicationFolderPath, $"{Path.GetFileNameWithoutExtension(currentApplicationFilePath)}.exe");
            }

            return currentApplicationFilePath;
        }

        #endregion
    }
}
