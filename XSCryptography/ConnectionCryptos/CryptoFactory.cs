using System;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public enum CryptoType
    {
        NoCrypto,
        EC,
        EC25519,
        RSA,
        RSALegacy
    }

    public class CryptoFactory
    {
        public static IConnectionCrypto CreateCrypto(CryptoType type, bool active)
        {
            switch(type)
            {
                case CryptoType.NoCrypto:
                    return new NoCrypto();
                case CryptoType.RSA:
                    return new RSACrypto(active);
                case CryptoType.RSALegacy:
                    return new RSALegacyCrypto(active);
                case CryptoType.EC:
                    return new ECCrypto(active);
                case CryptoType.EC25519:
                    return new EC25519Crypto(active);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
