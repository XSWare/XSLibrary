using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using XSLibrary.Cryptography.PasswordHashes;
using XSLibrary.DataBase;
using XSLibrary.Utility;

namespace XSLibrary.Cryptography.AccountManagement
{
    public class LocalSQLUserBase : IUserDataBase
    {
        SQLExecutor sqlExecutor;

        public string DatabaseName { get; private set; }
        public string DatabasePath { get; private set; }
        public string ServerConnectionString { get; private set; }
        public string ConnectionString { get; private set; }

        public LocalSQLUserBase(string databasePath, string serverString)
        {
            DatabasePath = databasePath;
            DatabaseName = Path.GetFileNameWithoutExtension(DatabasePath);
            ConnectionString = "Data Source=" + serverString + ";AttachDbFilename=" + databasePath + ";Integrated Security=True";
            ServerConnectionString = "Data Source=" + serverString + ";Initial Catalog=master; Integrated Security=true";

            sqlExecutor = new SQLExecutor(ConnectionString);

            InitializeDatabase();
        }

        private bool InitializeDatabase()
        {
            if (File.Exists(DatabasePath))
                return false;

            if (DatabaseExists())
                return false;

            CreateDatabase();
            return true;
        }

        private void CreateDatabase()
        {
            using (var connection = new SqlConnection(ServerConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = String.Format("CREATE DATABASE {0} ON PRIMARY (NAME={0}, FILENAME='{1}')", DatabaseName, DatabasePath);
                    command.ExecuteNonQuery();

                    command.CommandText = String.Format("EXEC sp_detach_db '{0}', 'true'", DatabaseName);
                    command.ExecuteNonQuery();
                }
            }

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = GetTableString();
                    command.ExecuteNonQuery();
                }
            }
        }

        private bool DatabaseExists()
        {
            using (var connection = new SqlConnection(ServerConnectionString))
            using (var command = new SqlCommand(string.Format("SELECT db_id(\'{0}\')", DatabaseName), connection))
            {
                connection.Open();
                return command.ExecuteScalar() != DBNull.Value;
            }
        }

        private string GetTableString()
        {
            return "CREATE TABLE [dbo].[Accounts] (" +
                "[id] INT IDENTITY(1, 1) NOT NULL," +
                "[username] VARCHAR(255) NOT NULL," +
                "[passwordhash] VARBINARY(MAX) NOT NULL," +
                "[salt] VARBINARY(MAX) NOT NULL," +
                "[difficulty] INT NOT NULL," +
                "[accesslevel] INT NOT NULL," +
                "[contact] VARCHAR(255)   NOT NULL," +
                "PRIMARY KEY CLUSTERED([id] ASC)," +
                "UNIQUE NONCLUSTERED([username] ASC)," +
                "UNIQUE NONCLUSTERED([contact] ASC));";
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
            using (SqlCommand command = new SqlCommand("DELETE FROM Accounts", sqlExecutor.Connection))
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
