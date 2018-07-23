using XSLibrary.Cryptography.PasswordHashes;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.Cryptography.AccountManagement
{
    public abstract class IUserDataBase
    {
        public int SaltLenth { get; set; } = 64;
        public int Difficulty { get; set; } = 10000;

        PasswordHash HashAlgorithm { get; set; }
        SafeReadWriteExecutor m_lock = new RWExecutor();

        public IUserDataBase()
        {
            HashAlgorithm = CreateHashAlgorithm();
        }

        public bool AddAccount(string username, byte[] password)
        {
            return m_lock.Execute(() => AddAccountUnsafe(username, password));
        }

        private bool AddAccountUnsafe(string username, byte[] password)
        {
            if (GetAccount(username) != null)
                return false;

            byte[] salt = GenerateSalt(SaltLenth);
            return AddUserData(new UserData(username, HashAlgorithm.Hash(password, salt, Difficulty), salt, Difficulty));
        }

        public void ReplaceAccount(string username, byte[] password)
        {
            m_lock.Execute(() => ReplaceAccountUnsafe(username, password));
        }

        private void ReplaceAccountUnsafe(string username, byte[] password)
        {
            EraseAccountUnsafe(username);
            AddAccountUnsafe(username, password);
        }

        public bool EraseAccount(string username)
        {
            return m_lock.Execute(() => EraseAccountUnsafe(username));
        }

        protected abstract bool AddUserData(UserData userData);
        protected abstract UserData GetAccount(string username);
        protected abstract bool EraseAccountUnsafe(string username);

        public bool ChangePassword(string username, byte[] oldPassword, byte[] newPassword)
        {
            return m_lock.Execute(() => ChangePasswordUnsafe(username, oldPassword, newPassword));
        }

        private bool ChangePasswordUnsafe(string username, byte[] oldPassword, byte[] newPassword)
        {
            if (!ValidateUnsafe(username, oldPassword))
                return false;

            ReplaceAccountUnsafe(username, newPassword);
            return true;
        }

        /// <summary>
        /// Check if the user data is in the data base and has a valid password
        /// </summary>
        public bool Validate(string username, byte[] password)
        {
            return m_lock.ExecuteRead(() => ValidateUnsafe(username, password));
        }

        private bool ValidateUnsafe(string username, byte[] password)
        {
            UserData user = GetAccount(username);
            if (user == null)
                return false;

            return AreHashesEqual(user.PasswordHash, HashAlgorithm.Hash(password, user.Salt, user.Difficulty));
        }

        protected abstract PasswordHash CreateHashAlgorithm();
        protected abstract byte[] GenerateSalt(int length);

        private bool AreHashesEqual(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }

            return true;
        }
    }
}
