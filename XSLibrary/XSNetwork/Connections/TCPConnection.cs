using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class TCPConnection : ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data);
        public event DataReceivedHandler DataReceivedEvent;

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
            byte[] data = new byte[MaxPacketSize];

            int size = ConnectionSocket.Receive(data, MaxPacketSize, SocketFlags.None);

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
            RaiseReceivedEvent(TrimData(data, size));
        }

        protected void RaiseReceivedEvent(byte[] data)
        {
            DataReceivedEvent?.Invoke(this, data);
        }
    }
}