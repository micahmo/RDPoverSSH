using System.Collections.ObjectModel;

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
    }
}
