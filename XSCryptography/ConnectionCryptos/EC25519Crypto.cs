using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    class EC25519Crypto : IConnectionCrypto
    {
        EC25519 KEXCrypto;
        AesCryptoServiceProvider DataCrypto;

        byte[] SECRET = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        public EC25519Crypto(bool active) : base(active)
        {
            KEXCrypto = new EC25519();

            DataCrypto = new AesCryptoServiceProvider();
            DataCrypto.Padding = PaddingMode.PKCS7;
        }

        protected override bool HandshakeActive(SendCall Send, ReceiveCall Receive)
        {
            Send(KEXCrypto.GetPublicKey());
            if (!Receive(out byte[] data, out EndPoint source))
                return false;

            DataCrypto.Key = KEXCrypto.GetSharedSecret(data);
            Send(DataCrypto.IV);

            if (!Receive(out data, out source))
                return false;

            return DecryptSecret(data);
        }

        private bool DecryptSecret(byte[] data)
        {
            byte[] decrypt = DecryptData(data);

            for (int i = 0; i < SECRET.Length && i < decrypt.Length; i++)
            {
                if (SECRET[i] != decrypt[i])
                    return false;
            }

            return true;
        }

        protected override bool HandshakePassive(SendCall Send, ReceiveCall Receive)
        {
            if (!Receive(out byte[] data, out EndPoint source))
                return false;

            DataCrypto.Key = KEXCrypto.GetSharedSecret(data);
            Send(KEXCrypto.GetPublicKey());

            if (!Receive(out data, out source))
                return false;

            DataCrypto.IV = data;
            Send(EncryptData(SECRET));

            return true;
        }

        public override byte[] EncryptData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, DataCrypto.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.Close();
                    return stream.ToArray();
                }
            }
        }
        public override byte[] DecryptData(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(stream, DataCrypto.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.Close();
                    return stream.ToArray();
                }
            }
        }
    }
}
