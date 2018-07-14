namespace XSLibrary.Cryptography.AccountManagement
{
    public abstract class IUserDataBase
    {
        public bool AddAccount(string userName, byte[] password)
        {
            if (GetAccount(userName) != null)
                return false;

            byte[] salt = GenerateSalt();
            AddUserData(new UserData(userName, Hash(password, salt), salt));

            return true;
        }

        protected abstract void AddUserData(UserData userData);

        protected abstract byte[] GenerateSalt();

        protected abstract UserData GetAccount(string userName);

        /// <summary>
        /// check if the user data is in the data base and has a valid password
        /// </summary>
        /// <param name="userData"></param>
        /// <returns></returns>
        public bool Validate(string userName, byte[] password)
        {
            UserData user = GetAccount(userName);
            if (user == null)
                return false;

            return AreHashesEqual(user.PasswordHash, Hash(password, user.Salt));
        }

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
