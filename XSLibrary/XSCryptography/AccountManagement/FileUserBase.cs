using System;
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

        protected override void AddUserData(UserData userData)
        {
            if (userData.Username.Contains(" ") || userData.Username.Contains("\n"))
                return;

            //if(!File.Exists(FilePath))
            //{
            //    File.Create(FilePath);
            //}

            //byte[] userByte = GetBytes(userData);
            System.IO.Directory.CreateDirectory(Directory);

            using (StreamWriter file = File.AppendText(FilePath))
            {
                file.WriteLine(UserToString(userData));
                file.Flush();
            }
            //FileStream stream = File.Open(FilePath, FileMode.Append);
            //stream.Write(userByte, stream.Length, userByte.Length);
        }

        private string UserToString(UserData user)
        {
            string passwordHash = HexStringConverter.ToString(user.PasswordHash);
            string salt = new SoapHexBinary(user.Salt).ToString();
            return string.Format("{0} {1} {2}", user.Username, passwordHash, salt);
        }

        //private byte[] GetBytes(UserData userdata)
        //{
        //    byte[] username = Encoding.ASCII.GetBytes(userdata.Username);

        //    byte[] output = new byte[userdata.Salt.Length + userdata.PasswordHash.Length + username.Length];

        //    Array.Copy(userdata.Salt, 0, output, 0, userdata.Salt.Length);
        //    Array.Copy(userdata.PasswordHash, 0, output, userdata.Salt.Length, userdata.PasswordHash.Length);
        //    Array.Copy(username, 0, output, userdata.Salt.Length + userdata.PasswordHash.Length, username.Length);

        //    return output;
        //}

        protected override UserData GetAccount(string userName)
        {
            System.IO.Directory.CreateDirectory(Directory);

            if (!File.Exists(FilePath))
                return null;

            string[] lines = File.ReadAllLines(FilePath);

            foreach(string userString in lines)
            {
                UserData user = StringToUser(userString);
                if (user.Username == userName)
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
