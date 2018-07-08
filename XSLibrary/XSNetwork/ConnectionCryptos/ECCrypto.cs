using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace XSLibrary.Network.ConnectionCryptos
{
    public class ECCrypto : IConnectionCrypto
    {
        ECDiffieHellmanCng KEXCrypto;
        AesCryptoServiceProvider DataCrypto;
        public bool Active { get; set; }

        byte[] SECRET = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        public ECCrypto(bool active)
        {
            Active = active;

            KEXCrypto = new ECDiffieHellmanCng(521);
            KEXCrypto.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            KEXCrypto.HashAlgorithm = CngAlgorithm.Sha256;

            DataCrypto = new AesCryptoServiceProvider();
            DataCrypto.Padding = PaddingMode.PKCS7;
        }

        public override bool Handshake(Action<byte[]> Send, ReceiveCall Receive)
        {
            if (Active)
                return HandshakeActive(Send, Receive);
            else
                return HandshakePassive(Send, Receive);
        }

        public bool HandshakeActive(Action<byte[]> Send, ReceiveCall Receive)
        {
            Send(KEXCrypto.PublicKey.ToByteArray());
            if (!Receive(out byte[] data, out IPEndPoint source))
                return false;

            DataCrypto.Key = KEXCrypto.DeriveKeyMaterial(CngKey.Import(data, CngKeyBlobFormat.EccPublicBlob));
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

        public bool HandshakePassive(Action<byte[]> Send, ReceiveCall Receive)
        {
            if (!Receive(out byte[] data, out IPEndPoint source))
                return false;

            DataCrypto.Key = KEXCrypto.DeriveKeyMaterial(CngKey.Import(data, CngKeyBlobFormat.EccPublicBlob));
            Send(KEXCrypto.PublicKey.ToByteArray());

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
