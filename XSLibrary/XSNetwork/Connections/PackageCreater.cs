using System;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        class PackageCreater
        {
            public int MaxPackageSize { get; set; } = 1440;     // usually safe for default MTU sizes

            byte[] currentData;
            int currentPos;

            public PackageCreater()
            {
                currentData = null;
                currentPos = 0;
            }

            public byte[][] SplitIntoPackages(byte[] data)
            {
                currentData = data;
                currentPos = 0;

                byte[][] packages = new byte[GetPackageCount()][];

                for (int i= 0; i < packages.Length; i++)
                {
                    packages[i] = CreateDataPackage();
                }

                if (currentPos != data.Length)
                    throw new Exception("Something went terribly wrong!");

                return packages;
            }

            private int GetPackageCount()
            {
                return currentData.Length / MaxPackageSize + (currentData.Length % MaxPackageSize != 0 ? 1 : 0);
            }

            private byte[] CreateDataPackage()
            {
                int length = Math.Min(currentData.Length - currentPos, MaxPackageSize);

                byte[] packet = new byte[Header_Size_ID + Header_Size_PacketLength + length];
                byte[] lengthHeader = BitConverter.GetBytes(length);

                packet[0] = Header_ID_Packet;
                Array.Copy(lengthHeader, 0, packet, Header_Size_ID, Header_Size_PacketLength);
                Array.Copy(currentData, currentPos, packet, Header_Size_Total, length);

                currentPos += length;

                return packet;
            }
        }
    }
}
