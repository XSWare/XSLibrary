using System;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        class PackageParser
        {
            public int MaxPackageSize { get; set; } = 2048;

            public bool NeedsFreshData { get; private set; }
            public bool PackageFinished { get; private set; }

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
                int leftoverSize = currentData.Length - (currentPos + currentPackage.Length);

                if (leftoverSize >= 0)
                {
                    Array.Copy(currentData, currentPos, currentPackage, 0, currentPackage.Length);
                    PackageFinished = true;

                    currentPos += currentPackage.Length;

                    if (currentPos >= currentData.Length)
                        NeedsFreshData = true;
                }
                else
                {
                    // insert rest of data into package and wait for fresh data
                    int currentDataLength = currentData.Length - currentPos;
                    currentPackagePos += currentDataLength;

                    Array.Copy(currentData, currentPos, currentPackage, currentPackagePos, currentDataLength);
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
