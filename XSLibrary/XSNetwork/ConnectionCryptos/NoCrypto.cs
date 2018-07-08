namespace XSLibrary.Network.ConnectionCryptos
{
    class NoCrypto : IConnectionCrypto
    {
        public bool Active { get; set; } = true;

        public bool Handshake() { return true; }

        public byte[] EncryptData(byte[] data) { return data; }
        public byte[] DecryptData(byte[] data) { return data; }
    }
}
