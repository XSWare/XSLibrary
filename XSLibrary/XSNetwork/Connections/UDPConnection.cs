using System.Net;
using System.Net.Sockets;

namespace XSLibrary.Network.Connections
{
    public class UDPConnection: ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, IPEndPoint endPoint);
        public event DataReceivedHandler DataReceivedEvent;

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp), local)
        {
        }

        public virtual void Send(byte[] data, IPEndPoint remoteEndPoint)
        {
            m_lock.Execute(() => UnsafeSend(data, remoteEndPoint));
        }

        private void UnsafeSend(byte[] data, IPEndPoint remoteEndPoint)
        {
            if (!Disconnecting)
                ConnectionSocket.SendTo(data, remoteEndPoint);
        }

        protected override void PreReceiveSettings()
        {
            ConnectionSocket.Bind(ConnectionEndpoint);
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[MaxPacketSize];
            EndPoint source = new IPEndPoint(ConnectionEndpoint.Address, ConnectionEndpoint.Port);

            int size = ConnectionSocket.ReceiveFrom(data, ref source);

            RaiseReceivedEvent(TrimData(data, size), source as IPEndPoint);    
        }

        private void RaiseReceivedEvent(byte[] data, IPEndPoint source)
        {
            DataReceivedHandler threadCopy = DataReceivedEvent;
            if (threadCopy != null)
                threadCopy.Invoke(this, data, source);
        }
    }
}
