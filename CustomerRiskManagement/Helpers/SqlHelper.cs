using System.Data;
using Microsoft.Data.SqlClient;

namespace CustomerRiskManagement.Helpers
{
    public class SqlServer
    {
        private readonly string _connectionString;

        public SqlServer(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LogoDb")!;
        }

        public DataTable GetDataTable(string query, SqlParameter[]? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            return dt;
        }

        public int Execute(string query, SqlParameter[]? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public object ExecuteScalar(string query, SqlParameter[]? parameters = null)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            conn.Open();

            return cmd.ExecuteScalar();
        }
    }
}