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

            byte[] headerLeftovers = new byte[Header_Size_Total];
            int headerLeftoverSize = 0;

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

                    currentPos += spacePackage;

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
                {
                    Logger.Log(LogLevel.Warning, "Received package has invalid size");
                    return false;
                }

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
                if (headerLeftoverSize > 0)
                    return CombineHeaderLeftovers();

                if (currentPos + Header_Size_Total > currentData.Length)
                {
                    StoreHeaderLeftovers();
                    return -1;
                }

                int size = ReadSize(currentData, currentPos + Header_Size_ID);
                currentPos += Header_Size_Total;
                return size;
            }

            private int CombineHeaderLeftovers()
            {
                int availableDataSize = currentData.Length - currentPos;
                if (availableDataSize == 0)
                    return -1;

                int requiredHeaderData = Header_Size_Total - headerLeftoverSize;

                if (requiredHeaderData > availableDataSize)
                {
                    Array.Copy(currentData, currentPos, headerLeftovers, headerLeftoverSize, availableDataSize);
                    return -1;
                }
                else
                {
                    Array.Copy(currentData, currentPos, headerLeftovers, headerLeftoverSize, requiredHeaderData);
                    headerLeftoverSize = 0;
                    currentPos += requiredHeaderData;
                    return ReadSize(headerLeftovers, Header_Size_ID);
                }
            }

            private void StoreHeaderLeftovers()
            {
                headerLeftoverSize = currentData.Length - currentPos;
                Array.Copy(currentData, currentPos, headerLeftovers, 0, headerLeftoverSize);
            }

            private int ReadSize(byte[] data, int offset)
            {
                return data[offset]
                    + (data[offset + 1] << 8)
                    + (data[offset + 2] << 16)
                    + (data[offset + 3] << 24);
            }

            private bool IsKeepAlive()
            {
                return currentData[currentPos] == Header_ID_KeepAlive;
            }

            private bool IsPacket()
            {
                return CheckHeaderLeftoversforPacketFlag() || currentData[currentPos] == Header_ID_Packet;
            }

            private bool CheckHeaderLeftoversforPacketFlag()
            {
                if (headerLeftoverSize <= 0)
                    return false;

                return headerLeftovers[0] == Header_ID_Packet;
            }
        }
    }
}
