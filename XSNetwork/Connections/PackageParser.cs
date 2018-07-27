using System;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        class PackageParser
        {
            public int MaxPackageSize { get; set; }

            public bool NeedsFreshData { get; private set; }
            public bool PackageFinished { get; private set; }

            public Logger Logger { get; set; } = Logger.NoLog;

            byte[] currentPackage;
            int currentPackagePos;
            byte[] currentData;
            int currentPos;

            public PackageParser()
            {
                NeedsFreshData = true;
                PackageFinished = false;
            }

            public byte[] GetPackage()
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

            public void ParsePackage()
            {
                if (NeedsFreshData)
                    return;

                if (currentPackage == null && !CreatePackage())
                {
                    NeedsFreshData = true;
                    return;
                }

                FillPackage();
            }

            private void FillPackage()
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

            private bool CreatePackage()
            {
                ConsumeKeepAlives();
                if (currentPos >= currentData.Length)
                    return false;

                if (!IsPacket())
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

            private void ConsumeKeepAlives()
            {
                while (currentPos < currentData.Length)
                {
                    if (IsKeepAlive())
                    {
                        currentPos += Header_Size_Total;
                        Logger.Log(LogLevel.Detail, "Received keep alive.");
                    }
                    else
                    {
                        return;
                    }
                }
            }

            int ParseSize()
            {
                if (currentPos + Header_Size_Total > currentData.Length)
                    return -1;

                return currentData[currentPos + Header_Size_ID]
                    + (currentData[currentPos + Header_Size_ID + 1] << 8)
                    + (currentData[currentPos + Header_Size_ID + 2] << 16)
                    + (currentData[currentPos + Header_Size_ID + 3] << 24);
            }

            private bool IsKeepAlive()
            {
                return currentData[currentPos] == Header_ID_KeepAlive;
            }

            private bool IsPacket()
            {
                return currentData[currentPos] == Header_ID_Packet;
            }
        }
    }
}
