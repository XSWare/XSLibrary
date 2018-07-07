using System;
using System.Collections.Generic;
using System.Net.Sockets;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public class TCPPacketConnection : TCPConnection
    {
        const byte Header_ID_Packet = 0x00;
        const byte Header_ID_KeepAlive = 0x01;
        const int Header_Size_ID = 1;
        const int Header_Size_PacketLength = 4;
        const int Header_Size_Total = Header_Size_ID + Header_Size_PacketLength;

        public TCPPacketConnection(Socket socket)
            : base(socket)
        {
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
            if (!Disconnecting)
            {
                ConnectionSocket.Send(new byte[] { Header_ID_KeepAlive, 0, 0, 0, 0 });
                Logger.Log("Sent keepalive.");
            }
        }

        protected override void ProcessReceivedData(byte[] data, int size)
        {
            byte[] trimmedData = TrimData(data, size);

            List<byte[]> packets = ParseIntoPacket(trimmedData);

            foreach (byte[] packet in packets)
            {
                Logger.Log("Received data.");
                RaiseReceivedEvent(packet);
            }
        }

        private List<byte[]> ParseIntoPacket(byte[] data)
        {
            List<byte[]> packets = new List<byte[]>();

            int currentPos = 0;

            while (currentPos < data.Length)
            {
                if (IsKeepAlive(data, currentPos))
                {
                    currentPos += Header_Size_Total;
                    continue;
                }

                if (!IsPacket(data, currentPos))
                    return packets;

                int packetSize = ParseSize(data, currentPos);
                if (packetSize < 0 || packetSize > MaxPacketSize || currentPos + Header_Size_PacketLength + packetSize > data.Length)
                    return packets;

                byte[] packet = new byte[packetSize];
                Array.Copy(data, currentPos + Header_Size_Total, packet, 0, packetSize);
                packets.Add(packet);

                currentPos += Header_Size_Total + packetSize;
            }

            return packets;
        }

        int ParseSize(byte[] data, int currentPos)
        {
            if (currentPos + Header_Size_Total > data.Length)
                return -1;

            return data[currentPos + Header_Size_ID]
                + (data[currentPos + Header_Size_ID + 1] << 8)
                + (data[currentPos + Header_Size_ID + 2] << 16)
                + (data[currentPos + Header_Size_ID + 3] << 24);
        }

        private bool IsKeepAlive(byte[] data, int currentPos)
        {
            return data[currentPos] == Header_ID_KeepAlive;
        }

        private bool IsPacket(byte[] data, int currentPos)
        {
            return data[currentPos] == Header_ID_Packet;
        }
    }
}
