using System.Net;
using System.Net.Sockets;

namespace XSLibrary.Network.Connections
{
    public class TCPConnection : IConnection
    {
        public TCPConnection(Socket socket) 
            : base(socket)
        {
            Local = socket.LocalEndPoint as IPEndPoint;
            Remote = socket.RemoteEndPoint as IPEndPoint;
        }

        protected override void SendSpecialized(byte[] data)
        {
            ConnectionSocket.Send(data);
        }

        protected override void PreReceiveSettings()
        {
            return;
        }

        protected override bool ReceiveSpecialized(out byte[] data, out EndPoint source)
        {
            data = new byte[ReceiveBufferSize];
            source = Remote;

            int size = ConnectionSocket.Receive(data, ReceiveBufferSize, SocketFlags.None);

            if (size <= 0)
            {
                ReceiveThread = null;
                return false;
            }

            TrimData(ref data, size);
            return true;
        }
    }
}