using System;
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

        public TunnelStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                LastStatusUpdateDateTime = DateTimeOffset.Now;
            }
        }
        private TunnelStatus _status;

        public string LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                LastStatusUpdateDateTime = DateTimeOffset.Now;
            }
        }
        private string _lastError;

        [BsonIgnore]
        public string TimestampedLastError => 
            $"[{(LastStatusUpdateDateTime.Date == DateTimeOffset.Now.Date ? $"{LastStatusUpdateDateTime.ToLocalTime():T}" : $"{LastStatusUpdateDateTime.ToLocalTime():G}")}] " +
            $"{LastError}";

        public Direction Direction { get; set; }

        public DateTimeOffset LastStatusUpdateDateTime { get; private set; }
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
        Disconnected,

        /// <summary>
        /// This status is intended for incoming reverse tunnels.
        /// It indicates that that the SSH server is running,
        ///  but no reverse tunnel is detected on the expected LocalTunnelPort
        /// </summary>
        Partial
    }
}
