using XSLibrary.Cryptography.PasswordHashes;
using XSLibrary.ThreadSafety.Executors;

namespace XSLibrary.Cryptography.AccountManagement
{
    public abstract class IUserDataBase
    {
        public int SaltLength { get; set; } = 64;
        public int Difficulty { get; set; } = 10000;

        PasswordHash HashAlgorithm { get; set; }
        SafeReadWriteExecutor m_lock = new RWExecutor();

        public IUserDataBase()
        {
            HashAlgorithm = CreateHashAlgorithm();
        }

        public bool AddAccount(AccountCreationData creationData)
        {
            return m_lock.Execute(() => AddAccountUnsafe(creationData));
        }

        private bool AddAccountUnsafe(AccountCreationData creationData)
        {
            if (GetAccount(creationData.Username) != null)
                return false;

            byte[] salt = GenerateSalt(SaltLength);

            return AddUserData(new AccountData(
                creationData.Username, 
                HashAlgorithm.Hash(creationData.Password, salt, Difficulty), 
                salt, 
                Difficulty, 
                creationData.AccessLevel));
        }

        public void ReplaceAccount(AccountCreationData creationData)
        {
            m_lock.Execute(() => ReplaceAccountUnsafe(creationData));
        }

        private void ReplaceAccountUnsafe(AccountCreationData creationData)
        {
            EraseAccountUnsafe(creationData.Username);
            AddAccountUnsafe(creationData);
        }

        public bool EraseAccount(string username)
        {
            return m_lock.Execute(() => EraseAccountUnsafe(username));
        }

        protected abstract bool AddUserData(AccountData userData);
        protected abstract AccountData GetAccount(string username);
        protected abstract bool EraseAccountUnsafe(string username);

        public bool ChangePassword(string username, byte[] oldPassword, byte[] newPassword)
        {
            return m_lock.Execute(() => ChangePasswordUnsafe(username, oldPassword, newPassword));
        }

        private bool ChangePasswordUnsafe(string username, byte[] oldPassword, byte[] newPassword)
        {
            AccountData account = GetAccount(username);
            if (account == null)
                return false;

            if (!ValidateHash(account, oldPassword))
                return false;

            AccountCreationData creationData = new AccountCreationData(username, newPassword, GetAccount(username).AccessLevel);
            ReplaceAccountUnsafe(creationData);
            return true;
        }

        /// <summary>
        /// Check if the user data is in the data base and has a valid password
        /// </summary>
        public bool Validate(string username, byte[] password, int accessLevel)
        {
            return m_lock.ExecuteRead(() =>
            {
                AccountData account = GetAccount(username);
                if (account == null)
                    return false;

                return ValidateHash(account, password) && ValidateAccesslevel(account, accessLevel);
            });
        }

        private bool ValidateHash(AccountData account, byte[] password)
        {
            return AreHashesEqual(account.PasswordHash, HashAlgorithm.Hash(password, account.Salt, account.Difficulty));
        }

        private bool ValidateAccesslevel(AccountData account, int accessLevel)
        {
            return accessLevel >= account.AccessLevel;
        }

        protected abstract PasswordHash CreateHashAlgorithm();
        protected abstract byte[] GenerateSalt(int length);

        private static bool AreHashesEqual(byte[] hash1, byte[] hash2)
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
