﻿using System.Net;
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
            SafeSend(() => ConnectionSocket.SendTo(Crypto.EncryptData(data), remote));
        }

        protected override void SendSpecialized(byte[] data)
        {
            if(Remote != null)
                ConnectionSocket.SendTo(data, Remote);
        }

        public void SetDefaultSend(IPEndPoint remote)
        {
            Remote = remote;
        }

        public void HolePunching(IPEndPoint remoteEndPoint)
        {
            SafeSend(() => ConnectionSocket.SendTo(new byte[0], remoteEndPoint));
            Logger.Log("Sent hole punching.");
        }

        protected override void PreReceiveSettings()
        {
            ConnectionSocket.Bind(Local);
        }

        protected override bool ReceiveSpecialized(out byte[] data, out IPEndPoint source)
        {
            data = new byte[MaxReceiveSize];
            EndPoint bufSource = new IPEndPoint(Local.Address, Local.Port);

            int size;
            do
            {
                size = ConnectionSocket.ReceiveFrom(data, ref bufSource);
                source = bufSource as IPEndPoint;
            }
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