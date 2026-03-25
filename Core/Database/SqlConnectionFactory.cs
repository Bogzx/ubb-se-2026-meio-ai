using Microsoft.Data.SqlClient;

namespace ubb_se_2026_meio_ai.Core.Database
{
    /// <summary>
    /// Default implementation of <see cref="ISqlConnectionFactory"/> 
    /// that creates connections to a local SQL Server instance.
    /// </summary>
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        // TODO: Move to appsettings or a configuration file before release.
        private const string DefaultConnectionString =
            "Server=BOGDANPC\\SQLEXPRESS;Database=MeioAiDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Application Name=MeioAi;";

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
    }
}
