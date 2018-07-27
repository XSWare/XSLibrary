using System;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    class Curve25519Crypto : IConnectionCrypto
    {
        //Ed25519 KEXCrypto;

        //byte[] privateKey;
        //byte[] publicKey;

        public Curve25519Crypto(bool active) : base(active)
        {
            //privateKey = Curve25519.CreateRandomPrivateKey();
            //KEXCrypto.
        }

        public override byte[] EncryptData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public override byte[] DecryptData(byte[] data)
        {
            throw new NotImplementedException();
        }

        protected override bool HandshakeActive(SendCall Send, ReceiveCall Receive)
        {
            throw new NotImplementedException();
        }

        protected override bool HandshakePassive(SendCall Send, ReceiveCall Receive)
        {
            throw new NotImplementedException();
        }
    }
}
