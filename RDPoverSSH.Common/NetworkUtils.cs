using System.Net;
using System.Net.Sockets;

namespace RDPoverSSH.Common
{
    public class NetworkUtils
    {
        /// <summary>
        /// Finds an open TCP port
        /// </summary>
        /// <remarks>
        /// From https://stackoverflow.com/a/150974/4206279
        /// </remarks>
        public static int GetFreeTcpPort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
    }
}
