using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Ubb_se_2026_meio_ai.Core.Database;
using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{
    public class ReelRepository
    {
        private const string SqlSelectUserReels = @"
                SELECT ReelId, MovieId, CreatorUserId, VideoUrl, ThumbnailUrl,
                       Title, Caption, FeatureDurationSeconds, BackgroundMusicId,
                       CropDataJson, Source, CreatedAt, LastEditedAt
                FROM Reel
                WHERE CreatorUserId = @UserId
                ORDER BY CreatedAt DESC";

        private const string SqlUpdateReelEdits = @"
                UPDATE Reel
                SET CropDataJson = @Crop,
                    BackgroundMusicId = @MusicId,
                    VideoUrl = COALESCE(@VideoUrl, VideoUrl),
                    LastEditedAt = SYSUTCDATETIME()
                WHERE ReelId = @ReelId";

        private const string SqlSelectReelById = @"
                SELECT ReelId, MovieId, CreatorUserId, VideoUrl, ThumbnailUrl,
                       Title, Caption, FeatureDurationSeconds, BackgroundMusicId,
                       CropDataJson, Source, CreatedAt, LastEditedAt
                FROM Reel
                WHERE ReelId = @ReelId";

        private const string SqlDeleteReel = @"
                DELETE FROM UserReelInteraction WHERE ReelId = @ReelId;
                DELETE FROM Reel WHERE ReelId = @ReelId;";

        private const string ParameterUserId = "@UserId";
        private const string ParameterCropData = "@Crop";
        private const string ParameterMusicId = "@MusicId";
        private const string ParameterVideoUrl = "@VideoUrl";
        private const string ParameterReelId = "@ReelId";

        private const int ColumnIndexReelId = 0;
        private const int ColumnIndexMovieId = 1;
        private const int ColumnIndexCreatorUserId = 2;
        private const int ColumnIndexVideoUrl = 3;
        private const int ColumnIndexThumbnailUrl = 4;
        private const int ColumnIndexTitle = 5;
        private const int ColumnIndexCaption = 6;
        private const int ColumnIndexFeatureDurationSeconds = 7;
        private const int ColumnIndexBackgroundMusicId = 8;
        private const int ColumnIndexCropDataJson = 9;
        private const int ColumnIndexSource = 10;
        private const int ColumnIndexCreatedAt = 11;
        private const int ColumnIndexLastEditedAt = 12;

        private readonly ISqlConnectionFactory sqlConnectionFactory;

        public ReelRepository(ISqlConnectionFactory sqlConnectionFactory)
        {
            this.sqlConnectionFactory = sqlConnectionFactory;
        }

        // Returns all reels where CreatorUserId = userId
        public async Task<IList<ReelModel>> GetUserReelsAsync(int userId)
        {
            var resultList = new List<ReelModel>();

            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlSelectUserReels, sqlConnection);
            sqlCommand.Parameters.AddWithValue(ParameterUserId, userId);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            while (await dataReader.ReadAsync())
            {
                resultList.Add(new ReelModel
                {
                    ReelId = dataReader.GetInt32(ColumnIndexReelId),
                    MovieId = dataReader.GetInt32(ColumnIndexMovieId),
                    CreatorUserId = dataReader.GetInt32(ColumnIndexCreatorUserId),
                    VideoUrl = dataReader.GetString(ColumnIndexVideoUrl),
                    ThumbnailUrl = dataReader.IsDBNull(ColumnIndexThumbnailUrl) ? string.Empty : dataReader.GetString(ColumnIndexThumbnailUrl),
                    Title = dataReader.GetString(ColumnIndexTitle),
                    Caption = dataReader.IsDBNull(ColumnIndexCaption) ? string.Empty : dataReader.GetString(ColumnIndexCaption),
                    FeatureDurationSeconds = dataReader.IsDBNull(ColumnIndexFeatureDurationSeconds) ? 0 : dataReader.GetDouble(ColumnIndexFeatureDurationSeconds),
                    BackgroundMusicId = dataReader.IsDBNull(ColumnIndexBackgroundMusicId) ? null : dataReader.GetInt32(ColumnIndexBackgroundMusicId),
                    CropDataJson = dataReader.IsDBNull(ColumnIndexCropDataJson) ? null : dataReader.GetString(ColumnIndexCropDataJson),
                    Source = dataReader.IsDBNull(ColumnIndexSource) ? string.Empty : dataReader.GetString(ColumnIndexSource),
                    CreatedAt = dataReader.GetDateTime(ColumnIndexCreatedAt),
                    LastEditedAt = dataReader.IsDBNull(ColumnIndexLastEditedAt) ? null : dataReader.GetDateTime(ColumnIndexLastEditedAt),
                });
            }
            return resultList;
        }

        // Updates CropDataJson, BackgroundMusicId, LastEditedAt and optionally VideoUrl for a reel.
        // Returns the number of rows affected (should be 1).
        public async Task<int> UpdateReelEditsAsync(int reelId, string cropDataJson, int? musicId, string? videoUrl = null)
        {
            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlUpdateReelEdits, sqlConnection);

            sqlCommand.Parameters.AddWithValue(ParameterCropData, cropDataJson);
            sqlCommand.Parameters.AddWithValue(ParameterMusicId, (object?)musicId ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue(ParameterVideoUrl, (object?)videoUrl ?? DBNull.Value);
            sqlCommand.Parameters.AddWithValue(ParameterReelId, reelId);

            return await sqlCommand.ExecuteNonQueryAsync();
        }

        public async Task<ReelModel?> GetReelByIdAsync(int reelId)
        {
            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlSelectReelById, sqlConnection);
            sqlCommand.Parameters.AddWithValue(ParameterReelId, reelId);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            if (!await dataReader.ReadAsync())
            {
                return null;
            }

            return new ReelModel
            {
                ReelId = dataReader.GetInt32(ColumnIndexReelId),
                MovieId = dataReader.GetInt32(ColumnIndexMovieId),
                CreatorUserId = dataReader.GetInt32(ColumnIndexCreatorUserId),
                VideoUrl = dataReader.GetString(ColumnIndexVideoUrl),
                ThumbnailUrl = dataReader.IsDBNull(ColumnIndexThumbnailUrl) ? string.Empty : dataReader.GetString(ColumnIndexThumbnailUrl),
                Title = dataReader.GetString(ColumnIndexTitle),
                Caption = dataReader.IsDBNull(ColumnIndexCaption) ? string.Empty : dataReader.GetString(ColumnIndexCaption),
                FeatureDurationSeconds = dataReader.IsDBNull(ColumnIndexFeatureDurationSeconds) ? 0 : dataReader.GetDouble(ColumnIndexFeatureDurationSeconds),
                BackgroundMusicId = dataReader.IsDBNull(ColumnIndexBackgroundMusicId) ? null : dataReader.GetInt32(ColumnIndexBackgroundMusicId),
                CropDataJson = dataReader.IsDBNull(ColumnIndexCropDataJson) ? null : dataReader.GetString(ColumnIndexCropDataJson),
                Source = dataReader.IsDBNull(ColumnIndexSource) ? string.Empty : dataReader.GetString(ColumnIndexSource),
                CreatedAt = dataReader.GetDateTime(ColumnIndexCreatedAt),
                LastEditedAt = dataReader.IsDBNull(ColumnIndexLastEditedAt) ? null : dataReader.GetDateTime(ColumnIndexLastEditedAt),
            };
        }

        // Deletes a reel from the database
        public async Task DeleteReelAsync(int reelId)
        {
            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlDeleteReel, sqlConnection);
            sqlCommand.Parameters.AddWithValue(ParameterReelId, reelId);
            await sqlCommand.ExecuteNonQueryAsync();
        }
    }
}