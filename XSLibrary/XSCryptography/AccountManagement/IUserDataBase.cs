namespace XSLibrary.Cryptography.AccountManagement
{
    public abstract class IUserDataBase
    {
        public bool AddAccount(string username, byte[] password)
        {
            if (GetAccount(username) != null)
                return false;

            byte[] salt = GenerateSalt();
            return AddUserData(new UserData(username, Hash(password, salt), salt));
        }

        public void ReplaceAccount(string username, byte[] password)
        {
            EraseAccount(username);
            AddAccount(username, password);
        }

        protected abstract bool AddUserData(UserData userData);

        protected abstract UserData GetAccount(string username);

        public abstract bool EraseAccount(string username);

        public bool ChangePassword(string username, byte[] oldPassword, byte[] newPassword)
        {
            if (!Validate(username, oldPassword))
                return false;

            ReplaceAccount(username, newPassword);
            return true;
        }

        /// <summary>
        /// check if the user data is in the data base and has a valid password
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool Validate(string username, byte[] password)
        {
            UserData user = GetAccount(username);
            if (user == null)
                return false;

            return AreHashesEqual(user.PasswordHash, Hash(password, user.Salt));
        }

        protected abstract byte[] GenerateSalt();

        protected abstract byte[] Hash(byte[] password, byte[] salt);

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
