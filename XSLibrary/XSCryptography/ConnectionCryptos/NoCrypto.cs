using System;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public class NoCrypto : IConnectionCrypto
    {
        public NoCrypto() : base(false) { }

        protected override bool HandshakeActive(Action<byte[]> Send, ReceiveCall Receive) { return true; }
        protected override bool HandshakePassive(Action<byte[]> Send, ReceiveCall Receive) { return true; }

        public override byte[] EncryptData(byte[] data) { return data; }
        public override byte[] DecryptData(byte[] data) { return data; }
    }
}
