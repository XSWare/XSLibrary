using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using XSLibrary.Utility;

namespace XSLibrary.Cryptography.AccountManagement
{
    public class FileUserBase : IUserDataBase
    {
        public int SaltLenth { get; set; } = 32;

        SHA512Cng HashAlgorithm = new SHA512Cng();

        public string Directory { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public FileUserBase(string directory, string fileName)
        {
            Directory = directory;
            FileName = fileName;
            FilePath = directory + fileName;
        }

        protected override bool AddUserData(UserData userData)
        {
            if (userData.Username.Contains(" ") || userData.Username.Contains("\n"))
                return false;

            System.IO.Directory.CreateDirectory(Directory);

            using (StreamWriter file = File.AppendText(FilePath))
            {
                file.WriteLine(UserToString(userData));
                file.Flush();
            }

            return true;
        }

        private string UserToString(UserData user)
        {
            string passwordHash = HexStringConverter.ToString(user.PasswordHash);
            string salt = new SoapHexBinary(user.Salt).ToString();
            return string.Format("{0} {1} {2}", user.Username, passwordHash, salt);
        }

        protected override UserData GetAccount(string username)
        {
            System.IO.Directory.CreateDirectory(Directory);

            if (!File.Exists(FilePath))
                return null;

            string[] lines = File.ReadAllLines(FilePath);

            foreach(string userString in lines)
            {
                UserData user = StringToUser(userString);
                if (user.Username == username)
                    return user;
            }

            return null;
        }

        private UserData StringToUser(string userString)
        {
            string[] split = userString.Split(' ');
            string username = split[0];
            byte[] passwordHash = HexStringConverter.ToBytes(split[1]);
            byte[] salt = HexStringConverter.ToBytes(split[2]);

            return new UserData(username, passwordHash, salt);
        }

        private string StringToUsername(string userString)
        {
            return userString.Split(' ')[0];
        }

        public override bool EraseAccount(string username)
        {
            System.IO.Directory.CreateDirectory(Directory);
            if (!File.Exists(FilePath))
                return false;

            bool erased = false;
            string[] lines = File.ReadAllLines(FilePath);

            List<string> writeBack = new List<string>();
            foreach(string userString in lines)
            {
                if (StringToUsername(userString) != username)
                    writeBack.Add(userString);
                else
                    erased = true;
            }

            File.WriteAllLines(FilePath, writeBack.ToArray());
            return erased;
        }

        protected override byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltLenth];
            RandomNumberGenerator.Create().GetBytes(salt);
            return salt;
        }

        protected override byte[] Hash(byte[] password, byte[] salt)
        {
            return HashAlgorithm.ComputeHash(SaltPassword(password, salt));
        }

        private byte[] SaltPassword(byte[] password, byte[] salt)
        {
            byte[] saltedPassword = new byte[password.Length + salt.Length];
            Array.Copy(salt, 0, saltedPassword, 0, salt.Length);
            Array.Copy(password, 0, saltedPassword, salt.Length, password.Length);
            return saltedPassword;
        }
    }
}
