using XSLibrary.Network.Connections;

namespace XSLibrary.Network.ConnectionCryptos
{
    class NoCrypto : IConnectionCrypto
    {
        public bool HandshakeActive(ConnectionInterface connection) { return true; }
        public bool HandshakePassive(ConnectionInterface connection) { return true; }

        public byte[] EncryptData(byte[] data) { return data; }
        public byte[] DecryptData(byte[] data) { return data; }
    }
}
