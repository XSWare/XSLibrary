using System.Net;
using System.Net.Sockets;

namespace XSLibrary.Network.Connections
{
    public class UDPConnection: ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data, IPEndPoint endPoint);
        public event DataReceivedHandler DataReceivedEvent;

        const int SIO_UDP_CONNRESET = -1744830452;

        IPEndPoint _local;
        protected override IPEndPoint Local => _local;

        IPEndPoint _remote;
        protected override IPEndPoint Remote => (_remote != null ? _remote : base.Remote);

        IPEndPoint _sendTarget;
        public IPEndPoint SendTarget
        {
            get { return (_sendTarget != null ? _sendTarget : Remote); }
            set { _sendTarget = value; }
        }

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            _local = local;

            // do this so remote cant close socket https://docs.microsoft.com/en-us/windows/desktop/WinSock/winsock-ioctls
            ConnectionSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }

        protected override void UnsafeSend(byte[] data)
        {
            if (!Disconnecting && Remote != null)
                ConnectionSocket.SendTo(data, SendTarget);
        }

        public void Send(byte[] data, IPEndPoint target)
        {
            IPEndPoint temp = SendTarget;
            SetDefaultSend(target);
            Send(data);
            SetDefaultSend(temp);
        }

        public void SetDefaultSend(IPEndPoint target)
        {
            SendTarget = target;
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
            EndPoint source = new IPEndPoint(Local.Address, Local.Port);

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