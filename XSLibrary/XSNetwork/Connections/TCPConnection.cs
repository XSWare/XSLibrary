using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class TCPConnection : ConnectionInterface
    {
        public delegate void DataReceivedHandler(object sender, byte[] data);
        public event DataReceivedHandler DataReceivedEvent;

        public IPEndPoint GetEndPoint { get { return ConnectionSocket.RemoteEndPoint as IPEndPoint; } }

        public TCPConnection(Socket socket) 
            : base(socket)
        {
        }

        public virtual void Send(byte[] data)
        {
            m_lock.Execute(() => UnsafeSend(data));
        }

        private void UnsafeSend(byte[] data)
        {
            if (!Disconnecting)
                ConnectionSocket.Send(data);
        }

        public void SendKeepAlive()
        {
            m_lock.Execute(UnsafeSendKeepAlive);
        }

        private void UnsafeSendKeepAlive()
        {
            if (!Disconnecting)
            {
                ConnectionSocket.Send(new byte[] { 123, 125 });
                Logger.Log("Sent keepalive.");
            }
        }

        protected override void PreReceiveSettings()
        {
            return;
        }

        protected override void ReceiveFromSocket()
        {
            byte[] data = new byte[MaxPacketSize];

            int size = ConnectionSocket.Receive(data);

            if (size <= 0)
            {
                ReceiveThread = null;
                ReceiveErrorHandling(ConnectionEndpoint);
                return;
            }

            Logger.Log("Received data.");
            RaiseReceivedEvent(TrimData(data, size));
        }

        void RaiseReceivedEvent(byte[] data)
        {
            DataReceivedHandler threadCopy = DataReceivedEvent;
            if (threadCopy != null)
                threadCopy.Invoke(this, data);
        }
    }
}
