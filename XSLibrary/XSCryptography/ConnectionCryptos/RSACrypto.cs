using System;
using System.Security.Cryptography;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    class RSACrypto : IConnectionCrypto
    {
        RSA KEXCrypto;
        AesCryptoServiceProvider DataCrypto;

        public RSACrypto(bool active) : base(active)
        {
            KEXCrypto = new RSACryptoServiceProvider(2048);
            //KEXCrypto.
        }

        protected override bool HandshakeActive(Action<byte[]> Send, ReceiveCall Receive)
        {
            throw new NotImplementedException();
        }

        protected override bool HandshakePassive(Action<byte[]> Send, ReceiveCall Receive)
        {
            throw new NotImplementedException();
        }

        public override byte[] EncryptData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] DecryptData(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
