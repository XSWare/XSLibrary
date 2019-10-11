using System;
using XSLibrary.Utility;

namespace XSLibrary.Network.Connections
{
    class PacketParser
    {
        public class PacketException : Exception 
        { 
            public PacketException() : base() { }
            public PacketException(string message) : base(message) { }
        }

        public bool NeedsFreshData { get { return currentData == null || currentPos == currentData.Length; } }
        public bool PackageFinished { get { return currentPackage != null && currentPackagePos == currentPackage.Length; } }

        public Logger Logger { get; set; } = Logger.NoLog;

        byte[] currentPackage;
        int currentPackagePos;
        byte[] currentData;
        int currentPos;

        public byte[] GetPacket(int packetSize, Func<byte[]> getData)
        {
            CreatePacket(packetSize);

            while (!PackageFinished)
            {
                if (NeedsFreshData)
                    AddData(getData());

                ParsePackage();
            }

            return FinalizePacket();
        }

        private byte[] FinalizePacket()
        {
            if (!PackageFinished)
                throw new PacketException("Packet finalized prematurely!");

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
                throw new PacketException("Trying to write data into uninitialized packet!");

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
            if (packetSize < 0)
                throw new PacketException(String.Format("Invalid packet size ({0} byte) detected!", packetSize));

            currentPackage = new byte[packetSize];
            currentPackagePos = 0;
        }
    }
}
