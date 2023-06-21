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

        public RootModel()
        {
            Connections.CollectionChanged += (_, __) =>
            {
                ConnectionsTotalCount = DatabaseEngine.GetCollection<ConnectionModel>().Count();
            };
        }

        /// <summary>
        /// The list of connections
        /// </summary>
        public ObservableCollection<ConnectionModel> Connections { get; } = new ObservableCollection<ConnectionModel>();

        public int ConnectionsTotalCount { get; private set; }

        public void Load(ExpressionStarter<ConnectionModel> connectionPredicate)
        {
            Connections.Clear();
            DatabaseEngine.GetCollection<ConnectionModel>().Find(connectionPredicate).OrderBy(c => c.Index).ToList().ForEach(Connections.Add);
        }
    }
}
