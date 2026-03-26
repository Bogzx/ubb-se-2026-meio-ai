using Microsoft.Data.SqlClient;

namespace ubb_se_2026_meio_ai.Core.Database
{
    /// <summary>
    /// Default implementation of <see cref="ISqlConnectionFactory"/> 
    /// that creates connections to a local SQL Server instance.
    /// </summary>
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        // Use SQL Server Express instance (visible in SSMS)
        private const string DefaultConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=andrei;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        private const string MasterConnectionString =
            @"Data Source=localhost\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";

        private readonly string _connectionString;

        public SqlConnectionFactory(string? connectionString = null)
        {
            _connectionString = connectionString ?? DefaultConnectionString;
        }

        public async Task<SqlConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<SqlConnection> CreateMasterConnectionAsync()
        {
            var connection = new SqlConnection(MasterConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}