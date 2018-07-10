using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.ThreadSafety.Executors;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        SafeExecutor m_sendLock;
        SafeExecutor m_receiveLock;

        public int MaxPackageSendSize
        {
            get { return Creater.MaxPackageSize; }
            set { m_sendLock.Execute(() => Creater.MaxPackageSize = value); }
        }

        public int MaxPackageReceiveSize
        {
            get { return Parser.MaxPackageSize; }
            set { m_receiveLock.Execute(() => Parser.MaxPackageSize = value); }
        }

        const byte Header_ID_Packet = 0x00;
        const byte Header_ID_KeepAlive = 0x01;
        const int Header_Size_ID = 1;
        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_ID + Header_Size_PacketLength;

        PackageCreater Creater;
        PackageParser Parser;

        public TCPPacketConnection(Socket socket)
            : base(socket)
        {
            m_sendLock = new SingleThreadExecutor();
            m_receiveLock = new SingleThreadExecutor();

            Creater = new PackageCreater();
            Parser = new PackageParser();
        }

        protected override void SendSpecialized(byte[] data)
        {
            m_sendLock.Execute(() =>
            {
                foreach (byte[] package in Creater.SplitIntoPackages(data))
                    ConnectionSocket.Send(package);
            });
        }

        public void SendKeepAlive()
        {
            SafeSend(() => ConnectionSocket.Send(new byte[] { Header_ID_KeepAlive, 0, 0, 0, 0 }));
            Logger.Log("Sent keepalive.");
        }

        protected override bool ReceiveSpecialized(out byte[] data, out IPEndPoint source)
        {
            m_receiveLock.Lock();

            try
            {
                data = null;
                source = Remote;

                while (!Parser.PackageFinished)
                {
                    if (Parser.NeedsFreshData)
                    {
                        if (!base.ReceiveSpecialized(out data, out source))
                            return false;

                        Parser.AddData(data);
                    }

                    Parser.ParsePackage();
                }

                data = Parser.GetPackage();
                return true;
            }
            finally { m_receiveLock.Release(); }
        }
    }
}
