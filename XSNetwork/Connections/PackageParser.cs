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

            public byte[] GetPacket(int packetSize, Func<byte[]> receive)
            {
                ConsumeKeepAlives();
                CreatePacket(packetSize);

                while (!PackageFinished)
                {
                    if (NeedsFreshData)
                        AddData(receive());

                    ParsePackage();
                }

                return FinalizePacket();
            }

            private byte[] FinalizePacket()
            {
                if (!PackageFinished)
                    throw new ConnectionException("Packet finalized prematurely!");

                byte[] package = currentPackage;
                currentPackage = null;
                PackageFinished = false;

                return package;
            }

            private void AddData(byte[] data)
            {
                if (!NeedsFreshData)
                    return;

                currentData = data;
                currentPos = 0;
                NeedsFreshData = false;
            }

            private void ParsePackage()
            {
                if (currentPackage == null)
                    throw new ConnectionException("Trying to write data into uninitialized packet!");

                if (NeedsFreshData)
                    return;

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

            public void CreatePacket(int packetSize)
            {
                if (packetSize < 0 || packetSize > MaxPackageSize)  // invalid package
                    throw new ConnectionException(String.Format("Invalid packet size ({0} byte) detected!", packetSize));

                currentPackage = new byte[packetSize];
                currentPackagePos = 0;
                PackageFinished = false;
            }

            private void ConsumeKeepAlives()
            {
                if (NeedsFreshData)
                    return;
                
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

            public static int ReadSize(byte[] data, int offset)
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
        }
    }
}
