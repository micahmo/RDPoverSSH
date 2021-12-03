using LiteDB;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// A model of the connection as maintained by the service
    /// </summary>
    public class ConnectionServiceModel : ModelBase<ConnectionServiceModel>
    {
        [BsonId]
        public int ObjectId { get; set; }

        public TunnelStatus Status { get; set; }

        public string LastError { get; set; }

        /// <summary>
        /// The port that we connect to locally to access the tunnel
        /// </summary>
        public int LocalTunnelPort { get; set; }
    }

    /// <summary>
    /// Describes the possible status of a tunnel connection
    /// </summary>
    public enum TunnelStatus
    {
        /// <summary>
        /// The connection status is not known, such as when it is first created
        /// </summary>
        Unknown,

        /// <summary>
        /// The tunnel is successfully connected
        /// </summary>
        Connected,

        /// <summary>
        /// The tunnel was unable to connect, or was connected and has become disconnected
        /// </summary>
        Disconnected
    }
}
