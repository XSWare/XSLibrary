using XSLibrary.Network.Connections;

namespace XSLibrary.Network.ConnectionCryptos
{
    public interface IConnectionCrypto
    {
        bool HandshakeActive(ConnectionInterface connection);
        bool HandshakePassive(ConnectionInterface connection);

        byte[] EncryptData(byte[] data);
        byte[] DecryptData(byte[] data);
    }
}
