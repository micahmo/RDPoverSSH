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
            if (Model.ConnectionPort != default)
            {
                if (DefaultConnectionPorts.FirstOrDefault(p => p.Value == Model.ConnectionPort) is PortViewModel selectedPortViewModel)
                {
                    SelectedConnectionPort = selectedPortViewModel;
                }
                else
                {
                    SelectedConnectionPort = PortViewModel.Custom;
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
