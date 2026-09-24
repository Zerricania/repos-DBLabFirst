using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace WindowsFormsApp1.Data
{
    /// <summary>
    /// Shared ADO.NET plumbing for every repository: opening a connection,
    /// running a parameterized command, and returning the raw result.
    /// Concrete repositories (BookRepository, CustomerRepository, ...) only
    /// write SQL and map DataRow -> their own model type; none of them touch
    /// SqlConnection/SqlCommand directly. This is the one place that changes
    /// if the data-access technology ever changes.
    /// </summary>
    public abstract class SqlRepositoryBase
    {
        protected static DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        protected static void ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (parameters != null) command.Parameters.AddRange(parameters);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        protected static DataRow FirstRowOrNull(DataTable table)
        {
            return table.Rows.Count > 0 ? table.Rows[0] : null;
        }

        protected static string AsString(DataRow row, string column)
        {
            object value = row[column];
            return value == DBNull.Value ? null : value.ToString();
        }

        protected static int? AsNullableInt(DataRow row, string column)
        {
            object value = row[column];
            return value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
        }
    }
}
