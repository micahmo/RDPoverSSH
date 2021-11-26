using System;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels
{
    public class ConnectionViewModel
    {
        #region Constructor

        public ConnectionViewModel(ConnectionModel model)
        {
            Model = model;
            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Any time the model changes, persist it
            DatabaseEngine.ConnectionCollection.Update(Model);

            if (e.PropertyName.Equals(nameof(Model.ConnectionDirection)))
            {
                ToggleConnectionDirectionCommand.IconGlyph = ConnectionDirectionGlyph;
            }
            else if (e.PropertyName.Equals(nameof(Model.TunnelDirection)))
            {
                ToggleTunnelDirectionCommand.IconGlyph = TunnelDirectionGlyph;
            }
        }

        #endregion

        #region Public properties

        public ConnectionModel Model { get; }

        public DeleteConnectionCommandViewModel DeleteConnectionCommand { get; } = new DeleteConnectionCommandViewModel();

        public GenericCommandViewModel ToggleConnectionDirectionCommand => _toggleConnectionDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleConnectionDirection), ConnectionDirectionGlyph);
        private GenericCommandViewModel _toggleConnectionDirectionCommand;

        public string ConnectionDirectionGlyph => Model.ConnectionDirection switch
        {
            Direction.Outgoing => "\xF0AF",
            Direction.Incoming => "\xF0B0",
            _ => "\xF0AF"
        };

        public GenericCommandViewModel ToggleTunnelDirectionCommand => _toggleTunnelDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleTunnelDirection), TunnelDirectionGlyph);
        private GenericCommandViewModel _toggleTunnelDirectionCommand;

        public string TunnelDirectionGlyph => Model.TunnelDirection switch
        {
            Direction.Outgoing => "\xF0AF",
            Direction.Incoming => "\xF0B0",
            _ => "\xF0AF"
        };

        public string MachineName => $"{Environment.MachineName} (Local Computer)";

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
