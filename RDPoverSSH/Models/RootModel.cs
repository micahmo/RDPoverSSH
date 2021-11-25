using System.Collections.ObjectModel;
using System.Linq;
using LinqKit;
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
            Connections.ToList().ForEach(c => DatabaseEngine.ConnectionCollection.Upsert(c));
        }

        public void Load(ExpressionStarter<ConnectionModel> connectionPredicate)
        {
            Connections.Clear();
            DatabaseEngine.ConnectionCollection.Find(connectionPredicate).ToList().ForEach(c => Connections.Add(c));
        }
    }
}
