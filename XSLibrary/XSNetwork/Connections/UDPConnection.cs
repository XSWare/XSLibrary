using System.Net;
using System.Net.Sockets;

namespace XSLibrary.Network.Connections
{
    public class UDPConnection: ConnectionInterface
    {
        const int SIO_UDP_CONNRESET = -1744830452;

        public UDPConnection(IPEndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            Local = local;

            // do this so remote cant close socket https://docs.microsoft.com/en-us/windows/desktop/WinSock/winsock-ioctls
            ConnectionSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }

        public void Send(byte[] data, IPEndPoint remote)
        {
            SetDefaultSend(remote);
            Send(data);
        }

        protected override bool CanSend()
        {
            return base.CanSend() && Remote != null;
        }

        protected override void SendSpecialized(byte[] data)
        {
                ConnectionSocket.SendTo(data, Remote);
        }

        public void SetDefaultSend(IPEndPoint remote)
        {
            Remote = remote;
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
            byte[] data = new byte[MaxReceiveSize];
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
    }
}