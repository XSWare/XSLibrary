using System;

namespace XSLibrary.Cryptography.ConnectionCryptos
{
    public enum CryptoType
    {
        NoCrypto,
        EC,
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
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
