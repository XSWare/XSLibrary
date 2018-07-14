namespace XSLibrary.Cryptography.AccountManagement
{
    class UserData
    {
        public string Username { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] Salt { get; private set; }

        public UserData(string userName, byte[] passwordHash, byte[] salt)
        {
            Username = userName;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
