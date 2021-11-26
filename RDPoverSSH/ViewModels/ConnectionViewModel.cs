using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
            if (Model.RemoteConnectionPort != default)
            {
                if (DefaultConnectionPorts.FirstOrDefault(p => p.Value == Model.RemoteConnectionPort) is PortViewModel selectedPortViewModel)
                {
                    SelectedRemoteConnectionPort = selectedPortViewModel;
                }
                else
                {
                    SelectedRemoteConnectionPort = PortViewModel.Custom;
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
                ToggleConnectionDirectionCommand.Description = ConnectionDirectionDescription;
            }
            else if (e.PropertyName.Equals(nameof(Model.TunnelDirection)))
            {
                ToggleTunnelDirectionCommand.IconGlyph = TunnelDirectionGlyph;
                ToggleConnectionDirectionCommand.Description = TunnelDirectionDescription;
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
            Direction.Outgoing => "This computer will connect to port X on the remote computer. Click to reverse.",
            Direction.Incoming => "The remote computer will connect to port X on this computer. Click to reverse.",
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
            Direction.Outgoing => "This computer will establish an SSH tunnel to the remote computer. Click to reverse.",
            Direction.Incoming => "The remote computer will establish an SSH tunnel to this computer. Click to reverse.",
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

        public PortViewModel SelectedRemoteConnectionPort
        {
            get => _selectedRemoteConnectionPort;
            set
            {
                SetProperty(ref _selectedRemoteConnectionPort, value);
                Model.RemoteConnectionPort = IsRemoteConnectionPortCustom
                    ? Model.RemoteConnectionPort // Don't change it
                    : _selectedRemoteConnectionPort.Value; // Change it to the predefined value
                OnPropertyChanged(nameof(IsRemoteConnectionPortCustom));
            }
        }
        private PortViewModel _selectedRemoteConnectionPort;

        public bool IsRemoteConnectionPortCustom => SelectedRemoteConnectionPort == PortViewModel.Custom;

        #endregion

        public GenericCommandViewModel ConnectionCommand => _connectionCommand ??=
            new GenericCommandViewModel("Connect", new RelayCommand(delegate { }), string.Empty);
        private GenericCommandViewModel _connectionCommand;

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

        #endregion
    }
}
