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
            @"Server=DESKTOP-2TK0CUF\SQLEXPRESS;Database=MeioAiDb;Trusted_Connection=True;TrustServerCertificate=True;";

        private const string MasterConnectionString =
            @"Server=DESKTOP-2TK0CUF\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

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