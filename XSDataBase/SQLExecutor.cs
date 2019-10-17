using System;
using System.Data.SqlClient;
using XSLibrary.Utility;

namespace XSLibrary.DataBase
{
    public class SQLExecutor : TransparentFunctionWrapper
    {
        public SqlConnection Connection { get; private set; } 

        public SQLExecutor(string connectionString)
        {
            Connection = new SqlConnection(connectionString);
        }

        public override void Execute(Action executeFunction)
        {
            Connection.Open();
            try
            {
                executeFunction();
            }
            finally { Connection.Close(); }
        }
    }
}
