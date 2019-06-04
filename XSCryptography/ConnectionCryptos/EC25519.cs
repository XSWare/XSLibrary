using XSLibrary.Cryptography.Algorithms;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    class EC25519
    {
        byte[] m_privateKey;

        public EC25519()
        {
            m_privateKey = Curve25519.CreateRandomPrivateKey();
        }

        public byte[] GetPublicKey()
        {
            return Curve25519.GetPublicKey(m_privateKey);
        }

        public byte[] GetSharedSecret(byte[] peerPublicKey)
        {
            return Curve25519.GetSharedSecret(m_privateKey, peerPublicKey);
        }
    }

}