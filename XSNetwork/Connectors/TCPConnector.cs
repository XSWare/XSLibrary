using System.Net.Sockets;
using XSLibrary.Network.Connections;

namespace XSLibrary.Network.Connectors
{
    public class TCPConnector : Connector<TCPConnection>
    {
        protected override TCPConnection InitializeConnection(Socket connectedSocket)
        {
            return new TCPConnection(connectedSocket);
        }
    }
}
