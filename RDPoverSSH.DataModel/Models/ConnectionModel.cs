using System;
using LiteDB;

namespace RDPoverSSH.Models
{
    /// <summary>
    /// Model representing the connection
    /// </summary>
    /// <remarks>
    /// Models get persisted on property changes, so all properties should be observable
    /// </remarks>
    public class ConnectionModel : ModelBase<ConnectionModel>
    {
        #region ModelBase members

        /// <inheritdoc/>
        public override Action<ILiteCollection<ConnectionModel>> Initialize { get; } = collection =>
        {
            // Our standing data for ConnectionModel is a single empty instance
            collection.Insert(new ConnectionModel());
        };

        #endregion

        [BsonId]
        public int ObjectId { get; set; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        } 
        private string _name;

        public Direction ConnectionDirection
        {
            get => _connectionDirection;
            set => SetProperty(ref _connectionDirection, value);
        }
        private Direction _connectionDirection;

        public Direction TunnelDirection
        {
            get => _tunnelDirection;
            set => SetProperty(ref _tunnelDirection, value);
        }
        private Direction _tunnelDirection;

        public string TunnelEndpoint
        {
            get => _tunnelEndpoint;
            set => SetProperty(ref _tunnelEndpoint, value);
        }
        private string _tunnelEndpoint;

        public int ConnectionPort
        {
            get => _connectionPort;
            set => SetProperty(ref _connectionPort, value);
        }
        private int _connectionPort;

        public int TunnelPort
        {
            get => _tunnelPort;
            set => SetProperty(ref _tunnelPort, value);
        }
        private int _tunnelPort;

        [BsonIgnore]
        public bool IsReverseTunnel => ConnectionDirection != TunnelDirection;

        /// <summary>
        /// The connection string used by SSH to establish the connection
        /// </summary>
        /// <remarks>
        /// The StrictHostKeyChecking should be re-enabled
        /// </remarks>
        public string TunnelConnectString => $"{TunnelEndpoint} -p {TunnelPort} -o StrictHostKeyChecking=no ServerAliveInterval=10";
    }

    /// <summary>
    /// Describes possible connections directions, including the tunnel
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// From this computer to the remote computer
        /// </summary>
        Outgoing,

        /// <summary>
        /// From the remote computer to this computer
        /// </summary>
        Incoming
    }

    /// <summary>
    /// Extensions on <see cref="Direction"/>.
    /// </summary>
    public static class DirectionExtensions
    {
        /// <summary>
        /// This method toggles the value of the <paramref name="direction"/> to the opposite direction.
        /// </summary>
        /// <remarks>
        /// This both changes the reference of the given parameter, as well as returning the new value,
        /// so that it can be used in places where the existing value can or cannot be used by ref.
        /// </remarks>
        public static Direction Toggle(ref this Direction direction)
        {
            direction = direction switch
            {
                Direction.Outgoing => Direction.Incoming,
                Direction.Incoming => Direction.Outgoing,
                _ => Direction.Outgoing
            };

            return direction;
        }
    }
}
