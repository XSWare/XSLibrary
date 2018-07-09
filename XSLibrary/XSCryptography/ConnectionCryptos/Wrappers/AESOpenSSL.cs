using OpenSSL.Crypto;

namespace XSLibrary.Cryptography.ConnectionCryptos.Wrappers
{
    class AESOpenSSL
    {
        CipherContext _aes;
        byte[] DerivedSecret { get; set; }
        public byte[] IV { get; private set; }

        public AESOpenSSL()
        {
            _aes = new CipherContext(Cipher.AES_256_CBC);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _aes.Encrypt(data, DerivedSecret, IV);
        }

        public byte[] Decrypt(byte[] data)
        {
            return _aes.Decrypt(data, DerivedSecret, IV);
        }

        public void SetSharedSecret(byte[] secret)
        {
            DerivedSecret = _aes.BytesToKey(MessageDigest.SHA256, null, secret, 32, out byte[] iv);
            IV = iv;
        }
    }
}
