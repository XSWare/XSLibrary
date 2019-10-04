using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        // this includes any cryptographic overhead as well so consider this while deciding its value
        public int MaxPackageReceiveSize { get; set; } = 2048;

        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_PacketLength;

        public override Logger Logger
        {
            get { return Parser.Logger; }
            set { Parser.Logger = value; }
        }

        PacketParser Parser = new PacketParser();

        public TCPPacketConnection(Socket socket)
            : base(socket)
        {
        }

        protected override void SendSpecialized(byte[] data)
        {
            ConnectionSocket.Send(CreateHeader(data.Length));
            ConnectionSocket.Send(data);
        }

        private byte[] CreateHeader(int contentSize)
        {
            byte[] header = new byte[Header_Size_Total];
            byte[] contentSizeBytes = BitConverter.GetBytes(contentSize);

            Array.Copy(contentSizeBytes, 0, header, 0, Header_Size_PacketLength);
            return header;
        }

        protected override bool ReceiveSpecialized(out byte[] data, out EndPoint source)
        {
            data = null;
            source = Remote;

            OneShotTimer timeout = null;
            int receiveTimeout = ReceiveTimeout;   // copy to avoid race condition
            if (receiveTimeout > 0)
                timeout = new OneShotTimer(receiveTimeout * 1000);

            Parser.MaxPackageSize = MaxPackageReceiveSize;

            while (!Parser.PackageFinished)
            {
                if (Parser.NeedsFreshData)
                {
                    if(timeout == null) // no timeout
                    {
                        if (!AsyncReceive(out data, 0))
                            return false;
                    }
                    else                // with timeout
                    {
                        int timeLeft = (int)timeout.TimeLeft.TotalMilliseconds;
                        if (timeLeft <= 0 || !AsyncReceive(out data, timeLeft))
                            return false;
                    }

                    Parser.AddData(data);
                }

                Parser.ParsePacket();
            }

            data = Parser.GetPacket();
            return true;
        }

        private bool AsyncReceive(out byte[] data, int timeout)
        {
            byte[] buffer = new byte[MaxReceiveSize];

            IAsyncResult receiveResult = ConnectionSocket.BeginReceive(buffer, 0, MaxReceiveSize, SocketFlags.None, null, null);

            bool success = false;
            if (timeout > 0)
                success = receiveResult.AsyncWaitHandle.WaitOne(timeout, true);
            else if (timeout == 0)
                success = receiveResult.AsyncWaitHandle.WaitOne();

            if (!success)
                ConnectionSocket.Dispose();

            int size = ConnectionSocket.EndReceive(receiveResult);
            success &= size > 0;

            if (success)
                data = TrimData(buffer, size);
            else
                data = null;

            return success;
        }
    }
}
