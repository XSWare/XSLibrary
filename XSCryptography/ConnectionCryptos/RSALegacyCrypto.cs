using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public class RSALegacyCrypto : IConnectionCrypto
    {
        RSACryptoServiceProvider KEXCrypto;
        AesCryptoServiceProvider DataCrypto;

        byte[] SECRET = Encoding.ASCII.GetBytes("y u stll us dis?");

        public RSALegacyCrypto(bool active) : base(active)
        {
            KEXCrypto = new RSACryptoServiceProvider(1024);
            DataCrypto = new AesCryptoServiceProvider();
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

        protected override bool HandshakeActive(SendCall Send, ReceiveCall Receive)
        {
            if(!Send(Encoding.ASCII.GetBytes(KEXCrypto.ToXmlString(false))))
                return false;
            if (!Receive(out byte[] data, out EndPoint source))
                return false;

            DataCrypto.Key = KEXCrypto.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            if(!Send(DataCrypto.IV))
                return false;

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

            KEXCrypto.FromXmlString(Encoding.ASCII.GetString(data));

            byte[] key = new byte[DataCrypto.KeySize / 8];
            RandomNumberGenerator.Create().GetBytes(key);
            DataCrypto.Key = key;

            if(!Send(KEXCrypto.Encrypt(key, RSAEncryptionPadding.Pkcs1)))
                return false;

            if (!Receive(out data, out source))
                return false;

            DataCrypto.IV = data;
            if(!Send(EncryptData(SECRET)))
                return false;

            return true;
        }
    }
}
