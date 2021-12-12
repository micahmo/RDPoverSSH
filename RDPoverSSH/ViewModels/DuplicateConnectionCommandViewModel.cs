using System.Windows.Input;
using LiteDB;
using Microsoft.Toolkit.Mvvm.Input;
using RDPoverSSH.Common;
using RDPoverSSH.DataStore;
using RDPoverSSH.Models;
using RDPoverSSH.Properties;

namespace RDPoverSSH.ViewModels
{
    public class DuplicateConnectionCommandViewModel : CommandViewModelBase
    {
        #region CommandViewModelBase

        /// <inheritdoc/>
        public override string Name => string.Empty;

        /// <inheritdoc/>
        public override string Description => Resources.DuplicateConnection;

        /// <inheritdoc/>
        public override ICommand Command => _command ??= new RelayCommand<object>(DeleteConnectionItem);
        private ICommand _command;

        /// <inheritdoc/>
        public override string IconGlyph => Icons.Copy;

        #endregion

        #region Command

        private void DeleteConnectionItem(object param)
        {
            if (param is ConnectionViewModel connectionViewModel)
            {
                BsonMapper bsonMapper = new BsonMapper();
                
                // Deep clone via serialization
                ConnectionModel duplicateConnection = bsonMapper.Deserialize<ConnectionModel>(bsonMapper.Serialize(connectionViewModel.Model));
                
                // Clear ObjectID so LiteDB sees this as a new object
                duplicateConnection.ObjectId = default;
                
                // Reset the tunnel port so that it is not shared with any other connection
                duplicateConnection.LocalTunnelPort = NetworkUtils.GetFreeTcpPort();
                
                // Add and save
                RootModel.Instance.Connections.Add(duplicateConnection);
                DatabaseEngine.GetCollection<ConnectionModel>().Insert(duplicateConnection);
            }
        }

        #endregion
    }
}
