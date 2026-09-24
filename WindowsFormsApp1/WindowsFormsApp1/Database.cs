using System.Configuration;
using Microsoft.Data.SqlClient;

namespace WindowsFormsApp1
{
    public static class Database
    {
        private const string ConnectionStringName = "BooksDb";

        public static SqlConnection GetConnection()
        {
            string connectionString = ConfigurationManager
                .ConnectionStrings[ConnectionStringName]
                .ConnectionString;

            return new SqlConnection(connectionString);
        }
    }
}
