using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace XSLibrary.Cryptography
{
    public class PasswordStorage
    {
        /// <summary>
        /// Encrypt a password that needs to be restored at some point. Don't use this to store login data - Use hashed passwords instead.
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt(string password)
        {
            byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
            byte[] bytes = Encoding.ASCII.GetBytes(password);
            return Convert.ToBase64String(ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser));
        }

        /// <summary>
        /// Decrypts a password that was previously encrypted with this class.
        /// </summary>
        /// <param name="password">Password in encrypted format.</param>
        /// <param name="result">Decrypted password.</param>
        /// <returns>Returns false if decryption failed.</returns>
        public static bool Decrypt(string password, out string result)
        {
            try
            {
                byte[] asString = Convert.FromBase64String(password);
                byte[] entropy = Encoding.ASCII.GetBytes(Assembly.GetExecutingAssembly().FullName);
                result = Encoding.ASCII.GetString(ProtectedData.Unprotect(asString, entropy, DataProtectionScope.CurrentUser));
                return true;
            }
            catch
            {
                result = "";
                return false;
            }
        }
    }
}
