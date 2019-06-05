using System.Security.Cryptography;

namespace XSLibrary.Cryptography.PasswordHashes
{
    public class SlowHashPBKDF2 : PasswordHash
    {
        public int HashLength { get; set; } = 64;
        public int Iterations { get; set; } = 20000;

        public override byte[] Hash(byte[] password, byte[] salt, int difficulty)
        {
            using (var PBKDF2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                return PBKDF2.GetBytes(HashLength);
        }
    }
}
