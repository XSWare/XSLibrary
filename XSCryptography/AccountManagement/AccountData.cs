namespace XSLibrary.Cryptography.AccountManagement
{
    public class AccountData
    {
        public string Username { get; private set; }
        public byte[] PasswordHash { get; private set; }
        public byte[] Salt { get; private set; }
        public int Difficulty { get; private set; }
        public int AccessLevel { get; private set; }
        public string Contact { get; private set; }

        public AccountData(string userName, byte[] passwordHash, byte[] salt, int difficulty, int accessLevel, string contact)
        {
            Username = userName;
            PasswordHash = passwordHash;
            Salt = salt;
            Difficulty = difficulty;
            AccessLevel = accessLevel;
            Contact = contact;
        }
    }

    public class AccountCreationData
    {
        public string Username { get; private set; }
        public byte[] Password { get; private set; }
        public int AccessLevel { get; private set; }
        public string Contact { get; private set; }

        public AccountCreationData(string userName, byte[] password, int accessLevel, string contact)
        {
            Username = userName;
            Password = password;
            AccessLevel = accessLevel;
            Contact = contact;
        }
    }
}
