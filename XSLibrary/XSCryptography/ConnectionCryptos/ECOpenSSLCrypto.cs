using System;
using System.Net;
using System.Text;
using XSLibrary.Cryptography.ConnectionCryptos.Wrappers;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public class ECOpenSSLCrypto : IConnectionCrypto
    {
        ECDHOpenSSL KEXCrypto;
        AESOpenSSL DataCrypto;

        string SECRET = "very secret, much wow!";

        public ECOpenSSLCrypto(bool active) : base(active)
        {
            KEXCrypto = new ECDHOpenSSL();
            DataCrypto = new AESOpenSSL();
        }

        public override byte[] EncryptData(byte[] data)
        {
            return DataCrypto.Encrypt(data);
        }

        public override byte[] DecryptData(byte[] data)
        {
            return DataCrypto.Decrypt(data);
        }

        protected override bool HandshakeActive(SendCall Send, ReceiveCall Receive)
        {
            Send(KEXCrypto.GetPublicKey());
            if (!Receive(out byte[] data, out IPEndPoint source))
                return false;

            DataCrypto.SetSharedSecret(KEXCrypto.GenerateSharedSecret(data));
            Send(EncryptData(Encoding.ASCII.GetBytes(SECRET)));
            return true;
        }

        protected override bool HandshakePassive(SendCall Send, ReceiveCall Receive)
        {
            if (!Receive(out byte[] data, out IPEndPoint source))
                return false;

            DataCrypto.SetSharedSecret(KEXCrypto.GenerateSharedSecret(data));
            Send(KEXCrypto.GetPublicKey());

            if (!Receive(out data, out source))
                return false;

            string secret = Encoding.ASCII.GetString(DecryptData(data));

            return secret == SECRET;
        }
    }
}
