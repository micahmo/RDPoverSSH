using System;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Models;

namespace RDPoverSSH.ViewModels
{
    public class ConnectionViewModel
    {
        #region Constructor

        public ConnectionViewModel(ConnectionModel model)
        {
            Model = model;

            Model.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName.Equals(nameof(Model.ConnectionDirection)))
                {
                    ToggleConnectionDirectionCommand.IconGlyph = ConnectionDirectionGlyph;
                }
                else if (args.PropertyName.Equals(nameof(Model.TunnelDirection)))
                {
                    ToggleTunnelDirectionCommand.IconGlyph = TunnelDirectionGlyph;
                }
            };
        }

        #endregion

        #region Public properties

        public ConnectionModel Model { get; }

        public DeleteConnectionItemViewModel DeleteConnectionCommand { get; } = new DeleteConnectionItemViewModel();

        public GenericCommandViewModel ToggleConnectionDirectionCommand => _toggleConnectionDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleConnectionDirection), ConnectionDirectionGlyph);
        private GenericCommandViewModel _toggleConnectionDirectionCommand;

        public string ConnectionDirectionGlyph => Model.ConnectionDirection switch
        {
            Direction.Normal => "\xF0AF",
            Direction.Reverse => "\xF0B0",
            _ => "\xF0AF"
        };

        public GenericCommandViewModel ToggleTunnelDirectionCommand => _toggleTunnelDirectionCommand ??=
            new GenericCommandViewModel(string.Empty, new RelayCommand(ToggleTunnelDirection), TunnelDirectionGlyph);
        private GenericCommandViewModel _toggleTunnelDirectionCommand;

        public string TunnelDirectionGlyph => Model.TunnelDirection switch
        {
            Direction.Normal => "\xF0AF",
            Direction.Reverse => "\xF0B0",
            _ => "\xF0AF"
        };

        public string MachineName => $"{Environment.MachineName} (local machine)";

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
