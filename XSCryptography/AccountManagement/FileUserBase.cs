﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using XSLibrary.Cryptography.PasswordHashes;
using XSLibrary.Utility;

namespace XSLibrary.Cryptography.AccountManagement
{
    public class FileUserBase : IUserDataBase
    {
        public string Directory { get; private set; }
        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public FileUserBase(string directory, string fileName)
        {
            Directory = directory;
            FileName = fileName;
            FilePath = directory + fileName;
        }

        protected override bool AddUserData(AccountData userData)
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

        private string UserToString(AccountData user)
        {
            string passwordHash = HexStringConverter.ToString(user.PasswordHash);
            string salt = new SoapHexBinary(user.Salt).ToString();
            string difficulty = Convert.ToString(user.Difficulty);
            string accessLevel = Convert.ToString(user.AccessLevel);
            return string.Format("{0} {1} {2} {3} {4}", user.Username, passwordHash, salt, difficulty, accessLevel);
        }

        protected override AccountData GetAccount(string username)
        {
            System.IO.Directory.CreateDirectory(Directory);

            if (!File.Exists(FilePath))
                return null;

            string[] lines = File.ReadAllLines(FilePath);

            foreach(string userString in lines)
            {
                AccountData user = StringToUser(userString);
                if (user.Username == username)
                    return user;
            }

            return null;
        }

        private AccountData StringToUser(string userString)
        {
            string[] split = userString.Split(' ');
            string username = split[0];
            byte[] passwordHash = HexStringConverter.ToBytes(split[1]);
            byte[] salt = HexStringConverter.ToBytes(split[2]);
            int difficulty = Convert.ToInt32(split[3]);
            int accessLevel = Convert.ToInt32(split[4]);

            return new AccountData(username, passwordHash, salt, difficulty, accessLevel);
        }

        private string StringToUsername(string userString)
        {
            return userString.Split(' ')[0];
        }

        protected override bool EraseAccountUnsafe(string username)
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

        protected override byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            RandomNumberGenerator.Create().GetBytes(salt);
            return salt;
        }

        protected override PasswordHash CreateHashAlgorithm()
        {
            return new SlowHashPBKDF2();
        }
    }
}