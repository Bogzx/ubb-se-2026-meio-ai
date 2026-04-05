using Microsoft.Data.SqlClient;

namespace Ubb_se_2026_meio_ai.Core.Database
{

    public interface ISqlConnectionFactory
    {

        Task<SqlConnection> CreateConnectionAsync();

        Task<SqlConnection> CreateMasterConnectionAsync();
    }
}