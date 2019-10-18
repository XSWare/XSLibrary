using System;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Security.Cryptography;
using XSLibrary.Cryptography.PasswordHashes;
using XSLibrary.DataBase;
using XSLibrary.Utility;

namespace XSLibrary.Cryptography.AccountManagement
{
    public class ServiceUserBase : IUserDataBase
    {
        SQLExecutor sqlExecutor;

        public ServiceUserBase(string connectionString)
        {
            sqlExecutor = new SQLExecutor(connectionString);
        }

        protected override bool AddUserData(AccountData userData)
        {
            string query = "INSERT INTO Accounts VALUES (@username, @passwordhash, @salt, @difficulty, @accesslevel, @contact)";

            try
            {
                SqlCommand command = new SqlCommand(query, sqlExecutor.Connection);

                command.Parameters.AddWithValue("@username", userData.Username);
                command.Parameters.AddWithValue("@passwordhash", userData.PasswordHash);
                command.Parameters.AddWithValue("@salt", userData.Salt);
                command.Parameters.AddWithValue("@difficulty", userData.Difficulty);
                command.Parameters.AddWithValue("@accesslevel", userData.AccessLevel);
                command.Parameters.AddWithValue("@contact", userData.Contact);

                return sqlExecutor.Execute(command.ExecuteNonQuery) > 0;
            }
            catch (SqlException ex)
            {
                Logger.Log(LogLevel.Error, "Error while adding to database: {0}", ex.Message);
            }

            return false;
        }

        protected override PasswordHash CreateHashAlgorithm()
        {
            return new SlowHashPBKDF2();
        }

        protected override bool EraseAccountUnsafe(string username)
        {
            using (SqlCommand command = new SqlCommand("DELETE FROM Accounts WHERE username=@username", sqlExecutor.Connection))
            {
                command.Parameters.AddWithValue("@username", username);
                return sqlExecutor.Execute(command.ExecuteNonQuery) > 0;
            }
        }

        protected override void EraseAllAccountsUnsafe()
        {
            using (SqlCommand command = new SqlCommand("DROP TABLE Accounts", sqlExecutor.Connection))
            {
                sqlExecutor.Execute(command.ExecuteNonQuery);
            }
        }

        protected override byte[] GenerateSalt(int length)
        {
            byte[] salt = new byte[length];
            RandomNumberGenerator.Create().GetBytes(salt);
            return salt;
        }

        protected override AccountData GetAccount(string username)
        {
            using (SqlCommand command = new SqlCommand("SELECT * FROM Accounts WHERE username=@username", sqlExecutor.Connection))
            {
                try
                {
                    sqlExecutor.Connection.Open();
                    command.Parameters.AddWithValue("@username", username);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            AccountData account = new AccountData(
                                reader["username"].ToString(),
                                (byte[])reader["passwordhash"],
                                (byte[])reader["salt"],
                                (int)reader["difficulty"],
                                (int)reader["accesslevel"],
                                reader["contact"].ToString());

                            return account;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(LogLevel.Error, "Error while reading account from database: " + ex.Message);
                }
                finally 
                { 
                    if(sqlExecutor.Connection.State == ConnectionState.Open)
                        sqlExecutor.Connection.Close(); 
                }
            }

            return null;
        }

        protected override bool IsAccountExisting(string username, string contact)
        {
            using (SqlCommand command = new SqlCommand("SELECT COUNT(1) FROM Accounts WHERE username=@username OR contact=@contact", sqlExecutor.Connection))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@contact", contact);
                return (int)sqlExecutor.Execute(command.ExecuteScalar) != 0;
            }
        }

        protected override bool IsCharacterAllowed(char character)
        {
            return true;
        }

        protected override string SanitizeData(string data)
        {
            return data;
        }

        protected override bool IsStringAllowed(string str)
        {
            return base.IsStringAllowed(str) && str.Length > 0 & str.Length <= 255;
        }
    }
}
