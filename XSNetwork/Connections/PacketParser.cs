using System;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        class PacketParser
        {
            public int MaxPackageSize { get; set; }

            public bool NeedsFreshData { get; private set; }
            public bool PackageFinished { get; private set; }

            public Logger Logger { get; set; } = Logger.NoLog;

            byte[] currentPackage;
            int currentPackagePos;
            byte[] currentData;
            int currentPos;

            public PacketParser()
            {
                NeedsFreshData = true;
                PackageFinished = false;
            }

            public byte[] GetPacket()
            {
                if (!PackageFinished)
                    return null;

                byte[] package = currentPackage;
                currentPackage = null;
                PackageFinished = false;

                return package;
            }

            public void AddData(byte[] data)
            {
                if (!NeedsFreshData)
                    return;

                currentData = data;
                currentPos = 0;
                NeedsFreshData = false;
            }

            public void ParsePacket()
            {
                if (NeedsFreshData)
                    return;

                if (currentPackage == null && !CreatePacket())
                {
                    NeedsFreshData = true;
                    return;
                }

                FillPacket();
            }

            private void FillPacket()
            {
                int spacePackage = currentPackage.Length - currentPackagePos;
                int currentDataLeft = currentData.Length - currentPos;
                int leftoverDataSize = currentDataLeft - spacePackage;

                if (leftoverDataSize >= 0)  // enough data to fill the package
                {
                    Array.Copy(currentData, currentPos, currentPackage, currentPackagePos, spacePackage);
                    PackageFinished = true;

                    currentPos += currentPackage.Length;

                    if (currentPos == currentData.Length)   // package is full and data is empty -> get more data for next
                        NeedsFreshData = true;
                }
                else    // not enough data to fill package
                {
                    // insert rest of data into package and wait for fresh data
                    Array.Copy(currentData, currentPos, currentPackage, currentPackagePos, currentDataLeft);
                    currentPackagePos += currentDataLeft;

                    NeedsFreshData = true;
                }
            }

            private bool CreatePacket()
            {
                if (currentPos >= currentData.Length)
                    return false;

                int packetSize = ParseSize();
                if (packetSize < 0 || packetSize > MaxPackageSize)  // invalid package
                    return false;

                currentPos += Header_Size_Total;

                PackageFinished = false;
                currentPackage = new byte[packetSize];
                currentPackagePos = 0;

                return true;
            }

            int ParseSize()
            {
                if (currentPos + Header_Size_Total > currentData.Length)
                    return -1;

                return currentData[currentPos]
                    + (currentData[currentPos + 1] << 8)
                    + (currentData[currentPos + 2] << 16)
                    + (currentData[currentPos + 3] << 24);
            }
        }
    }
}
