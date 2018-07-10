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

                for (int i = 0; i < packages.Length; i++)
                {
                    if (i == 0)
                        packages[i] = CreateHeaderPackage();
                    else
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

            private byte[] CreateHeaderPackage()
            {
                int dataLength = Math.Min(currentData.Length - currentPos, MaxPackageSize);

                byte[] header = new byte[Header_Size_ID + Header_Size_PacketLength + dataLength];
                byte[] lengthHeader = BitConverter.GetBytes(currentData.Length);

                header[0] = Header_ID_Packet;
                Array.Copy(lengthHeader, 0, header, Header_Size_ID, Header_Size_PacketLength);

                FillPackage(header, Header_Size_Total, dataLength);

                return header;
            }

            private byte[] CreateDataPackage()
            {
                int length = Math.Min(currentData.Length - currentPos, MaxPackageSize);

                byte[] packet = new byte[length];

                FillPackage(packet, 0, length);

                return packet;
            }

            private void FillPackage(byte[] package, int offset, int length)
            {
                Array.Copy(currentData, currentPos, package, offset, length);
                currentPos += length;
            }
        }
    }
}
