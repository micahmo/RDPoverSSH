using System.Collections.ObjectModel;
using System.Linq;
using RDPoverSSH.DataStore;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// The root model as a singleton that provides access to all the other data models
    /// </summary>
    public class RootModel
    {
        #region Singleton

        /// <summary>
        /// The singleton instance
        /// </summary>
        public static RootModel Instance { get; } = new RootModel();

        #endregion

        /// <summary>
        /// The list of connections
        /// </summary>
        public ObservableCollection<ConnectionModel> Connections { get; } = new ObservableCollection<ConnectionModel>();

        public void Save()
        {
            var objectIds = Connections.Select(c => c.ObjectId).ToList();
            DatabaseEngine.ConnectionCollection.DeleteMany(c => !objectIds.Contains(c.ObjectId));
            Connections.ToList().ForEach(c => DatabaseEngine.ConnectionCollection.Upsert(c));
        }

        public void Load()
        {
            Connections.Clear();
            DatabaseEngine.ConnectionCollection.Query().ToList().ForEach(c => Connections.Add(c));
        }
    }
}
