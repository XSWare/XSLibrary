using System;
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

        /// <param name="waitTime">specifies the time (in milliseconds) between the last communication and the first keep alive attempt</param>
        /// <param name="interval">interval (in milliseconds) between each keep alive attempt</param>
        public void SetUpKeepAlive(int waitTime, int interval)
        {
            byte[] inputValues = new byte[3 * sizeof(uint)];
            BitConverter.GetBytes(1).CopyTo(inputValues, 0);    // enable flag
            BitConverter.GetBytes(waitTime).CopyTo(inputValues, sizeof(uint));
            BitConverter.GetBytes(interval).CopyTo(inputValues, sizeof(uint) * 2);
            ConnectionSocket.IOControl(IOControlCode.KeepAliveValues, inputValues, null);
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
            data = new byte[MaxReceiveSize];
            source = Remote;

            int size = ConnectionSocket.Receive(data, MaxReceiveSize, SocketFlags.None);

            if (size <= 0)
            {
                ReceiveThread = null;
                return false;
            }

            data = TrimData(data, size);
            return true;
        }
    }
}