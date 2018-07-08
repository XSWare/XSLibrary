using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using XSLibrary.Network.Connections;

namespace XSLibrary.Network.ConnectionCryptos
{
    public class ECCrypto : IConnectionCrypto
    {
        ConnectionInterface Connection { get; set; }

        ECDiffieHellmanCng KEXCrypto;
        AesCryptoServiceProvider DataCrypto;
        ManualResetEvent m_finishedEvent;
        public bool Active { get; set; }

        byte[] SECRET = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        int Timeout = 500000;

        public ECCrypto(ConnectionInterface connection, bool active)
        {
            Connection = connection;
            Active = active;

            m_finishedEvent = new ManualResetEvent(false);

            KEXCrypto = new ECDiffieHellmanCng(521);
            KEXCrypto.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            KEXCrypto.HashAlgorithm = CngAlgorithm.Sha256;

            DataCrypto = new AesCryptoServiceProvider();
            DataCrypto.Padding = PaddingMode.PKCS7;
        }

        public bool Handshake()
        {
            if (Active)
                return HandshakeActive();
            else
                return HandshakePassive();
        }

        public bool HandshakeActive()
        {
            Connection.DataReceivedEvent += ProcessPassivePublicKey;
            Connection.Send(KEXCrypto.PublicKey.ToByteArray());
            return m_finishedEvent.WaitOne(Timeout);
        }

        private void ProcessPassivePublicKey(object sender, byte[] publicKey, IPEndPoint source)
        {
            DataCrypto.Key = KEXCrypto.DeriveKeyMaterial(CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob));
            Connection.DataReceivedEvent -= ProcessPassivePublicKey;
            Connection.DataReceivedEvent += DecryptSecret;
            Connection.Send(DataCrypto.IV);
        }

        private void DecryptSecret(object sender, byte[] secret, IPEndPoint source)
        {
            byte[] decrypt = DecryptData(secret);

            for(int i = 0; i < SECRET.Length && i < decrypt.Length; i++)
            {
                if (SECRET[i] != decrypt[i])
                    return;
            }

            m_finishedEvent.Set();
        }

        public bool HandshakePassive()
        {
            Connection.DataReceivedEvent += ProcessActivePublicKey;
            //return m_finishedEvent.WaitOne(Timeout);
            return true;
        }

        private void ProcessActivePublicKey(object sender, byte[] publicKey, IPEndPoint source)
        {
            Connection.DataReceivedEvent -= ProcessActivePublicKey;
            CngKey key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] aesKey = KEXCrypto.DeriveKeyMaterial(key);
            DataCrypto.Key = aesKey;
            Connection.DataReceivedEvent += ProcessIV;
            Connection.Send(KEXCrypto.PublicKey.ToByteArray());
        }

        private void ProcessIV(object sender, byte[] IV, IPEndPoint source)
        {
            Connection.DataReceivedEvent -= ProcessIV;
            DataCrypto.IV = IV;
            Connection.Send(EncryptData(SECRET));
        }

        public byte[] EncryptData(byte[] data)
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
        public byte[] DecryptData(byte[] data)
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
