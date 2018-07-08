using System;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public class NoCrypto : IConnectionCrypto
    {
        public override bool Handshake(Action<byte[]> Send, ReceiveCall Receive) { return true; }

        public override byte[] EncryptData(byte[] data) { return data; }
        public override byte[] DecryptData(byte[] data) { return data; }
    }
}
