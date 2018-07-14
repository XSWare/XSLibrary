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

        const byte Header_ID_Packet = 0x00;
        const byte Header_ID_KeepAlive = 0x01;
        const int Header_Size_ID = 1;
        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_ID + Header_Size_PacketLength;

        PackageParser Parser;

        public TCPPacketConnection(Socket socket)
            : base(socket)
        {
            Parser = new PackageParser();
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
                Logger.Log("Sent keepalive.");
        }

        protected override bool ReceiveSpecialized(out byte[] data, out EndPoint source)
        {
            data = null;
            source = Remote;

            Parser.MaxPackageSize = MaxPackageReceiveSize;

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
    }
}
