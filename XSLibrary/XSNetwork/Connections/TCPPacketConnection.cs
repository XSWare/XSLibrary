using System;
using System.Net;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        const byte Header_ID_Packet = 0x00;
        const byte Header_ID_KeepAlive = 0x01;
        const int Header_Size_ID = 1;
        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_ID + Header_Size_PacketLength;

        PackageParser Parser;

        public TCPPacketConnection(Socket socket, int maxPacketSize = 2048)
            : base(socket)
        {
            Parser = new PackageParser(maxPacketSize);
        }

        protected override void SendSpecialized(byte[] data)
        {
            if (!Disconnecting)
                ConnectionSocket.Send(CreateDataPackage(data));
        }

        private byte[] CreateDataPackage(byte[] data)
        {
            byte[] packet = new byte[Header_Size_ID + Header_Size_PacketLength + data.Length];
            byte[] lengthHeader = BitConverter.GetBytes(data.Length);

            packet[0] = Header_ID_Packet;
            Array.Copy(lengthHeader, 0, packet, Header_Size_ID, Header_Size_PacketLength);
            Array.Copy(data, 0, packet, Header_Size_Total, data.Length);

            return packet;
        }

        public void SendKeepAlive()
        {
            m_lock.Execute(UnsafeSendKeepAlive);
        }

        private void UnsafeSendKeepAlive()
        {
            if (CanSend())
            {
                ConnectionSocket.Send(new byte[] { Header_ID_KeepAlive, 0, 0, 0, 0 });
                Logger.Log("Sent keepalive.");
            }
        }

        protected override bool ReceiveFromSocket(out byte[] data, out IPEndPoint source)
        {
            data = null;
            source = Remote;

            if(Parser.NeedsFreshData)
            {
                if (!base.ReceiveFromSocket(out data, out source))
                    return false;

                Parser.AddData(data);
            }

            Parser.ParsePackage();
            if (!Parser.PackageFinished)
                return false;

            data = Parser.GetPackage();
            return true;
        }
    }
}
