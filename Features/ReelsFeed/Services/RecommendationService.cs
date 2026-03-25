using Microsoft.Data.SqlClient;
using ubb_se_2026_meio_ai.Core.Database;
using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.ReelsFeed.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public RecommendationService(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IList<ReelModel>> GetRecommendedReelsAsync(int userId, int count)
        {
            var reels = new List<ReelModel>();
            const string sql = @"
                SELECT TOP (@count)
                    ReelId, MovieId, CreatorUserId, VideoUrl, ThumbnailUrl, Title, 
                    Caption, FeatureDurationSeconds, CropDataJson, BackgroundMusicId, 
                    Source, CreatedAt, LastEditedAt
                FROM Reel
                ORDER BY CreatedAt DESC
            ";

            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@count", count);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                reels.Add(new ReelModel
                {
                    ReelId = reader.GetInt32(0),
                    MovieId = reader.GetInt32(1),
                    CreatorUserId = reader.GetInt32(2),
                    VideoUrl = reader.GetString(3),
                    ThumbnailUrl = reader.GetString(4),
                    Title = reader.GetString(5),
                    Caption = reader.GetString(6),
                    FeatureDurationSeconds = reader.GetDouble(7),
                    CropDataJson = reader.IsDBNull(8) ? null : reader.GetString(8),
                    BackgroundMusicId = reader.IsDBNull(9) ? null : reader.GetInt32(9),
                    Source = reader.GetString(10),
                    CreatedAt = reader.GetDateTime(11),
                    LastEditedAt = reader.IsDBNull(12) ? null : reader.GetDateTime(12)
                });
            }

            return reels;
        }
    }
}
