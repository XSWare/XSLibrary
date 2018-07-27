using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class UDPConnection: IConnection
    {
        const int SIO_UDP_CONNRESET = -1744830452;

        EndPoint ReceiveSource { get; set; }

        public UDPConnection(EndPoint local) : base(new Socket(local.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
        {
            
            Local = local;

            // do this so remote cant close socket https://docs.microsoft.com/en-us/windows/desktop/WinSock/winsock-ioctls
            ConnectionSocket.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0, 0, 0, 0 }, null);
        }

        public void Send(byte[] data, EndPoint remote)
        {
            SafeSend(() => ConnectionSocket.SendTo(Crypto.EncryptData(data), remote));
        }

        protected override void SendSpecialized(byte[] data)
        {
            if (Remote == null)
                new ConnectionException("Set default target before sending or choose a different send call!");

            ConnectionSocket.SendTo(data, Remote);
        }

        public void SetDefaultSend(EndPoint remote)
        {
            Remote = remote;
        }

        public void HolePunching(EndPoint remoteEndPoint)
        {
            SafeSend(() => ConnectionSocket.SendTo(new byte[0], remoteEndPoint));
            Logger.Log(LogLevel.Detail, "Sent hole punching.");
        }

        protected override void PreReceiveSettings()
        {
            ConnectionSocket.Bind(Local);
        }

        protected override bool ReceiveSpecialized(out byte[] data, out EndPoint source)
        {
            data = new byte[MaxReceiveSize];
            source = Local;

            int size;
            do { size = ConnectionSocket.ReceiveFrom(data, ref source); }
            while (IsHolePunching(size));

            data = TrimData(data, size);
            return true;
        }

        private bool IsHolePunching(int size)
        {
            return size == 0;
        }
    }
}