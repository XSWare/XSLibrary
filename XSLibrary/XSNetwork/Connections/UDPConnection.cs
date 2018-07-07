using System.Net;
using System.Net.Sockets;

namespace XSLibrary.Network.Connections
{
    public class UDPConnection: ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, IPEndPoint endPoint);
        public event DataReceivedHandler DataReceivedEvent;

        const int SIO_UDP_CONNRESET = -1744830452;

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp), local)
        {
            _local = local;
            // do this so remote cant close socket https://docs.microsoft.com/en-us/windows/desktop/WinSock/winsock-ioctls
            ConnectionSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }

        protected override void UnsafeSend(byte[] data)
        {
            if (!Disconnecting && Remote != null)
                ConnectionSocket.SendTo(data, Remote);
        }

        private void UnsafeSend(byte[] data, IPEndPoint remoteEndPoint)
        {
            if (!Disconnecting)
                ConnectionSocket.SendTo(data, remoteEndPoint);
        }

        public void HolePunching(IPEndPoint remoteEndPoint)
        {
            Send(new byte[0], remoteEndPoint);
        }

        protected override void PreReceiveSettings()
        {
            ConnectionSocket.Bind(Local);
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[MaxPacketSize];
            EndPoint source = new IPEndPoint(ConnectionEndpoint.Address, ConnectionEndpoint.Port);

            int size = ConnectionSocket.ReceiveFrom(data, ref source);

            if(IsHolePunching(size))
                return;

            RaiseReceivedEvent(TrimData(data, size), source as IPEndPoint);    
        }

        private bool IsHolePunching(int size)
        {
            return size == 0;
        }

        private void RaiseReceivedEvent(byte[] data, IPEndPoint source)
        {
            DataReceivedEvent?.Invoke(this, data, source);
        }
    }
}

        public event DataReceivedHandler DataReceivedEvent;

        const int SIO_UDP_CONNRESET = -1744830452;

        IPEndPoint _local;
        protected override IPEndPoint Local { get { return _local; } }

        IPEndPoint _remote;
        protected override IPEndPoint Remote { get { return (_remote != null ? _remote : base.Remote); } }

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        public void Send(byte[] data, IPEndPoint target)
        {
            IPEndPoint temp = Remote;
            SetPermanentSendTarget(target);
            Send(data);
            SetPermanentSendTarget(temp);
        public void SetPermanentSendTarget(IPEndPoint target)
        {
            _remote = target;
        }

        public void HolePunching(IPEndPoint remoteEndPoint)
        {
            Send(new byte[0], remoteEndPoint);
            byte[] data = new byte[MaxPacketSize];
            EndPoint source = Local;

            int size = ConnectionSocket.ReceiveFrom(data, ref source);

            if(IsHolePunching(size))
                return;
