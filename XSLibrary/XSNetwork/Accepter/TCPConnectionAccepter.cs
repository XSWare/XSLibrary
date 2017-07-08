using System.Net.Sockets;

namespace XSLibrary.Network.Accepters
{
    public class TCPAccepter : Accepter
    {
        public TCPAccepter(int port, int maxNumberClients) : 
            base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), port, maxNumberClients)
        {
        }
    }
}
