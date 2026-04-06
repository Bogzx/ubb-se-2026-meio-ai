using Microsoft.Data.SqlClient;
using Ubb_se_2026_meio_ai.Core.Database;
using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{
    public class AudioLibraryService : IAudioLibraryService
    {
        private const string SqlSelectAllTracks = "SELECT MusicTrackId, TrackName, Author, AudioUrl, DurationSeconds FROM MusicTrack ORDER BY TrackName";
        private const string SqlSelectTrackById = "SELECT MusicTrackId, TrackName, Author, AudioUrl, DurationSeconds FROM MusicTrack WHERE MusicTrackId = @Id";
        private const string ParameterTrackId = "@Id";

        private const int ColumnIndexMusicTrackId = 0;
        private const int ColumnIndexTrackName = 1;
        private const int ColumnIndexAuthor = 2;
        private const int ColumnIndexAudioUrl = 3;
        private const int ColumnIndexDurationSeconds = 4;

        private readonly ISqlConnectionFactory sqlConnectionFactory;

        public AudioLibraryService(ISqlConnectionFactory sqlConnectionFactory)
        {
            this.sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<IList<MusicTrackModel>> GetAllTracksAsync()
        {
            var resultList = new List<MusicTrackModel>();

            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlSelectAllTracks, sqlConnection);
            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            while (await dataReader.ReadAsync())
            {
                resultList.Add(new MusicTrackModel
                {
                    MusicTrackId = dataReader.GetInt32(ColumnIndexMusicTrackId),
                    TrackName = dataReader.GetString(ColumnIndexTrackName),
                    Author = dataReader.IsDBNull(ColumnIndexAuthor) ? string.Empty : dataReader.GetString(ColumnIndexAuthor),
                    AudioUrl = dataReader.GetString(ColumnIndexAudioUrl),
                    DurationSeconds = dataReader.GetDouble(ColumnIndexDurationSeconds),
                });
            }

            return resultList;
        }

        public async Task<MusicTrackModel?> GetTrackByIdAsync(int musicTrackId)
        {
            await using var sqlConnection = await sqlConnectionFactory.CreateConnectionAsync();
            await using var sqlCommand = new SqlCommand(SqlSelectTrackById, sqlConnection);
            sqlCommand.Parameters.AddWithValue(ParameterTrackId, musicTrackId);

            await using var dataReader = await sqlCommand.ExecuteReaderAsync();

            if (await dataReader.ReadAsync())
            {
                return new MusicTrackModel
                {
                    MusicTrackId = dataReader.GetInt32(ColumnIndexMusicTrackId),
                    TrackName = dataReader.GetString(ColumnIndexTrackName),
                    Author = dataReader.IsDBNull(ColumnIndexAuthor) ? string.Empty : dataReader.GetString(ColumnIndexAuthor),
                    AudioUrl = dataReader.GetString(ColumnIndexAudioUrl),
                    DurationSeconds = dataReader.GetDouble(ColumnIndexDurationSeconds),
                };
            }

            return null;
        }
    }
}