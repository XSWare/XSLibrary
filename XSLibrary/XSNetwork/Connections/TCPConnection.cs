﻿using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class TCPConnection : ConnectionInterface
    {
        public TCPConnection(Socket socket) 
            : base(socket)
        {
            Local = socket.LocalEndPoint as IPEndPoint;
            Remote = socket.RemoteEndPoint as IPEndPoint;
        }

        protected override void SendSpecialized(byte[] data)
        {
            if (!Disconnecting)
                ConnectionSocket.Send(data);
        }

        protected override void PreReceiveSettings()
        {
            return;
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[MaxReceiveSize];

            int size = ConnectionSocket.Receive(data, MaxReceiveSize, SocketFlags.None);

            if (size <= 0)
            {
                ReceiveThread = null;
                ReceiveErrorHandling(Remote);
                return;
            }

            Logger.Log("Received data.");
            ProcessReceivedData(data, size);
        }

        protected virtual void ProcessReceivedData(byte[] data, int size)
        {
            RaiseReceivedEvent(TrimData(data, size), Remote);
        }
    }
}