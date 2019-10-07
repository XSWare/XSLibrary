using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        // this includes any cryptographic overhead as well so consider this while deciding its value
        public int MaxPacketReceiveSize { get; set; } = 2048;

        const byte Header_ID_Packet = 0x00;
        const byte Header_ID_KeepAlive = 0x01;
        const int Header_Size_ID = 1;
        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_ID + Header_Size_PacketLength;

        public override Logger Logger
        {
            get { return Parser.Logger; }
            set { Parser.Logger = value; }
        }

        PackageParser Parser = new PackageParser();

        public TCPPacketConnection(Socket socket)
            : base(socket)
        {
        }

        protected override void SendSpecialized(byte[] data)
        {
            ConnectionSocket.Send(CreateHeader(data.Length));
            ConnectionSocket.Send(data);
        }

        private byte[] CreateHeader(int length)
        {
            byte[] header = new byte[Header_Size_ID + Header_Size_PacketLength];
            byte[] lengthHeader = BitConverter.GetBytes(length);

            header[0] = Header_ID_Packet;
            Array.Copy(lengthHeader, 0, header, Header_Size_ID, Header_Size_PacketLength);
            return header;
        }

        public void SendKeepAlive()
        {
            if(SafeSend(() => ConnectionSocket.Send(new byte[] { Header_ID_KeepAlive, 0, 0, 0, 0 })))
                Logger.Log(LogLevel.Detail, "Sent keep alive.");
        }

        public void StartKeepAliveLoop(int loopInterval, int checkInterval)
        {
            DebugTools.ThreadpoolStarter("Keep alive loop", () =>
            {
                int currentWaitTime = 0;
                while (Connected)
                {
                    Thread.Sleep(checkInterval);

                    currentWaitTime += checkInterval;
                    if (currentWaitTime >= loopInterval)
                    {
                        currentWaitTime = 0;
                        SendKeepAlive();
                    }
                }
            });
        }

        protected override bool ReceiveSpecialized(out byte[] data, out EndPoint source)
        {
            data = null;
            source = Remote;

            OneShotTimer timeout = null;
            int receiveTimeout = ReceiveTimeout;   // copy to avoid race condition
            if (receiveTimeout > 0)
                timeout = new OneShotTimer(receiveTimeout * 1000);

            Parser.MaxPackageSize = MaxPacketReceiveSize;

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

                Parser.ParsePackage();
            }

            data = Parser.GetPackage();
            return true;
        }

        private bool AsyncReceive(out byte[] data, int timeout)
        {
            data = new byte[ReceiveBufferSize];

            IAsyncResult receiveResult = ConnectionSocket.BeginReceive(data, 0, ReceiveBufferSize, SocketFlags.None, null, null);

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
                TrimData(ref data, size);
            else
                data = null;

            return success;
        }
    }
}
