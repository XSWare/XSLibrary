using OpenSSL.Core;
using OpenSSL.Crypto;
using OpenSSL.Crypto.EC;
using System;

namespace XSLibrary.Cryptography.ConnectionCryptos.Wrappers
{
    class ECDHOpenSSL : IDisposable 
    {
        Key _ecdh;
        Group _group { get { return _ecdh.Group; } }
        BigNumber.Context _context;

        public ECDHOpenSSL()
        {
            _ecdh = Key.FromCurveName(Objects.NID.secp521r1);
            _ecdh.GenerateKey();

            _context = new BigNumber.Context();
        }

        public byte[] GenerateSharedSecret(byte[] otherPublicKey)
        {
            byte[] sharedSecret = new byte[32];
            _ecdh.ComputeKey(new Point(_group, otherPublicKey, _context), sharedSecret, SHA256);
            return sharedSecret;
        }

        public byte[] GetPublicKey()
        {
            return _ecdh.PublicKey.GetBytes(_context, Point.Form.POINT_CONVERSION_UNCOMPRESSED);
        }

        private byte[] SHA256(byte[] msg)
        {
            using (MessageDigestContext mdc = new MessageDigestContext(MessageDigest.SHA256))
            {
                return mdc.Digest(msg);
            }
        }

        public void Dispose()
        {
            _ecdh.Dispose();
            _context.Dispose();
        }
    }
}
