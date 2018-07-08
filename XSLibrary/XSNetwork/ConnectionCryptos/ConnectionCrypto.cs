namespace XSLibrary.Network.ConnectionCryptos
{
    public interface IConnectionCrypto
    {
        bool Active { get; set; }

        bool Handshake();

        byte[] EncryptData(byte[] data);
        byte[] DecryptData(byte[] data);
    }
}
