namespace XSLibrary.Cryptography.PasswordHashes
{
    public abstract class PasswordHash
    {
        public abstract byte[] Hash(byte[] password, byte[] salt, int difficulty);
    }
}
