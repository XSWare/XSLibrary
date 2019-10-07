using System;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    public partial class TCPPacketConnection : TCPConnection
    {
        class PackageParser
        {
            public int MaxPackageSize { get; set; }

            public bool NeedsFreshData { get { return currentData == null || currentPos == currentData.Length; } }
            public bool PackageFinished { get { return currentPackage != null && currentPackagePos == currentPackage.Length; } }

            public Logger Logger { get; set; } = Logger.NoLog;

            byte[] currentPackage;
            int currentPackagePos;
            byte[] currentData;
            int currentPos;

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

                return package;
            }

            private void AddData(byte[] data)
            {
                if (!NeedsFreshData)
                    return;

                currentData = data;
                currentPos = 0;
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

                WriteDataToPacket(Math.Min(currentDataLeft, spacePackage));
            }

            private void WriteDataToPacket(int size)
            {
                Array.Copy(currentData, currentPos, currentPackage, currentPackagePos, size);
                currentPos += size;
                currentPackagePos += size;
            }

            public void CreatePacket(int packetSize)
            {
                if (packetSize < 0 || packetSize > MaxPackageSize)
                    throw new ConnectionException(String.Format("Invalid packet size ({0} byte) detected!", packetSize));

                currentPackage = new byte[packetSize];
                currentPackagePos = 0;
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
