using Microsoft.Data.SqlClient;
using ubb_se_2026_meio_ai.Core.Database;
using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Raw SQL implementation of <see cref="IScrapeJobRepository"/>.
    /// Owner: Andrei
    /// </summary>
    public class ScrapeJobRepository : IScrapeJobRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ScrapeJobRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> CreateJobAsync(ScrapeJobModel job)
        {
            const string sql = @"
                INSERT INTO ScrapeJob (SearchQuery, MaxResults, Status, MoviesFound, ReelsCreated, StartedAt)
                VALUES (@SearchQuery, @MaxResults, @Status, @MoviesFound, @ReelsCreated, @StartedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@SearchQuery", job.SearchQuery);
            cmd.Parameters.AddWithValue("@MaxResults", job.MaxResults);
            cmd.Parameters.AddWithValue("@Status", job.Status);
            cmd.Parameters.AddWithValue("@MoviesFound", job.MoviesFound);
            cmd.Parameters.AddWithValue("@ReelsCreated", job.ReelsCreated);
            cmd.Parameters.AddWithValue("@StartedAt", job.StartedAt);

            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task UpdateJobAsync(ScrapeJobModel job)
        {
            const string sql = @"
                UPDATE ScrapeJob
                SET Status          = @Status,
                    MoviesFound     = @MoviesFound,
                    ReelsCreated    = @ReelsCreated,
                    CompletedAt     = @CompletedAt,
                    ErrorMessage    = @ErrorMessage
                WHERE ScrapeJobId   = @ScrapeJobId;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", job.Status);
            cmd.Parameters.AddWithValue("@MoviesFound", job.MoviesFound);
            cmd.Parameters.AddWithValue("@ReelsCreated", job.ReelsCreated);
            cmd.Parameters.AddWithValue("@CompletedAt", (object?)job.CompletedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ErrorMessage", (object?)job.ErrorMessage ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ScrapeJobId", job.ScrapeJobId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddLogEntryAsync(ScrapeJobLogModel log)
        {
            const string sql = @"
                INSERT INTO ScrapeJobLog (ScrapeJobId, Level, Message, Timestamp)
                VALUES (@ScrapeJobId, @Level, @Message, @Timestamp);";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ScrapeJobId", log.ScrapeJobId);
            cmd.Parameters.AddWithValue("@Level", log.Level);
            cmd.Parameters.AddWithValue("@Message", log.Message);
            cmd.Parameters.AddWithValue("@Timestamp", log.Timestamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<IList<ScrapeJobModel>> GetAllJobsAsync()
        {
            const string sql = "SELECT * FROM ScrapeJob ORDER BY StartedAt DESC;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            var jobs = new List<ScrapeJobModel>();
            while (await reader.ReadAsync())
            {
                jobs.Add(MapJob(reader));
            }
            return jobs;
        }

        public async Task<IList<ScrapeJobLogModel>> GetLogsForJobAsync(int jobId)
        {
            const string sql = "SELECT * FROM ScrapeJobLog WHERE ScrapeJobId = @ScrapeJobId ORDER BY Timestamp;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ScrapeJobId", jobId);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            var logs = new List<ScrapeJobLogModel>();
            while (await reader.ReadAsync())
            {
                logs.Add(MapLog(reader));
            }
            return logs;
        }

        public async Task<IList<ScrapeJobLogModel>> GetAllLogsAsync()
        {
            const string sql = "SELECT TOP 200 * FROM ScrapeJobLog ORDER BY Timestamp DESC;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            var logs = new List<ScrapeJobLogModel>();
            while (await reader.ReadAsync())
            {
                logs.Add(MapLog(reader));
            }
            return logs;
        }

        public async Task<DashboardStatsModel> GetDashboardStatsAsync()
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Movie)                                            AS TotalMovies,
                    (SELECT COUNT(*) FROM Reel)                                             AS TotalReels,
                    (SELECT COUNT(*) FROM ScrapeJob)                                        AS TotalJobs,
                    (SELECT COUNT(*) FROM ScrapeJob WHERE Status = 'running')               AS RunningJobs,
                    (SELECT COUNT(*) FROM ScrapeJob WHERE Status = 'completed')             AS CompletedJobs,
                    (SELECT COUNT(*) FROM ScrapeJob WHERE Status = 'failed')                AS FailedJobs;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            await using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            var stats = new DashboardStatsModel();
            if (await reader.ReadAsync())
            {
                stats.TotalMovies   = reader.GetInt32(reader.GetOrdinal("TotalMovies"));
                stats.TotalReels    = reader.GetInt32(reader.GetOrdinal("TotalReels"));
                stats.TotalJobs     = reader.GetInt32(reader.GetOrdinal("TotalJobs"));
                stats.RunningJobs   = reader.GetInt32(reader.GetOrdinal("RunningJobs"));
                stats.CompletedJobs = reader.GetInt32(reader.GetOrdinal("CompletedJobs"));
                stats.FailedJobs    = reader.GetInt32(reader.GetOrdinal("FailedJobs"));
            }
            return stats;
        }

        public async Task<int?> FindMovieByTitleAsync(string title)
        {
            const string sql = "SELECT TOP 1 MovieId FROM Movie WHERE Title = @Title;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", title);

            object? result = await cmd.ExecuteScalarAsync();
            return result is null or DBNull ? null : Convert.ToInt32(result);
        }

        public async Task<int> InsertMovieAsync(string title, string posterUrl, string genre, string description, int releaseYear)
        {
            const string sql = @"
                INSERT INTO Movie (Title, PosterUrl, PrimaryGenre, Description, ReleaseYear)
                VALUES (@Title, @PosterUrl, @PrimaryGenre, @Description, @ReleaseYear);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@PosterUrl", posterUrl);
            cmd.Parameters.AddWithValue("@PrimaryGenre", genre);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.AddWithValue("@ReleaseYear", releaseYear);

            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> ReelExistsByVideoUrlAsync(string videoUrl)
        {
            const string sql = "SELECT COUNT(*) FROM Reel WHERE VideoUrl = @VideoUrl;";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VideoUrl", videoUrl);

            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        public async Task<int> InsertScrapedReelAsync(ReelModel reel)
        {
            const string sql = @"
                INSERT INTO Reel (MovieId, CreatorUserId, VideoUrl, ThumbnailUrl, Title, Caption, Source, CreatedAt)
                VALUES (@MovieId, @CreatorUserId, @VideoUrl, @ThumbnailUrl, @Title, @Caption, @Source, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using SqlConnection conn = await _connectionFactory.CreateConnectionAsync();
            await using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MovieId", reel.MovieId);
            cmd.Parameters.AddWithValue("@CreatorUserId", reel.CreatorUserId);
            cmd.Parameters.AddWithValue("@VideoUrl", reel.VideoUrl);
            cmd.Parameters.AddWithValue("@ThumbnailUrl", reel.ThumbnailUrl);
            cmd.Parameters.AddWithValue("@Title", reel.Title);
            cmd.Parameters.AddWithValue("@Caption", reel.Caption);
            cmd.Parameters.AddWithValue("@Source", reel.Source);
            cmd.Parameters.AddWithValue("@CreatedAt", reel.CreatedAt);

            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ── Private helpers ──────────────────────────────────────────────

        private static ScrapeJobModel MapJob(SqlDataReader reader)
        {
            return new ScrapeJobModel
            {
                ScrapeJobId     = reader.GetInt32(reader.GetOrdinal("ScrapeJobId")),
                SearchQuery     = reader.GetString(reader.GetOrdinal("SearchQuery")),
                MaxResults      = reader.GetInt32(reader.GetOrdinal("MaxResults")),
                Status          = reader.GetString(reader.GetOrdinal("Status")),
                MoviesFound     = reader.GetInt32(reader.GetOrdinal("MoviesFound")),
                ReelsCreated    = reader.GetInt32(reader.GetOrdinal("ReelsCreated")),
                StartedAt       = reader.GetDateTime(reader.GetOrdinal("StartedAt")),
                CompletedAt     = reader.IsDBNull(reader.GetOrdinal("CompletedAt"))
                                    ? null : reader.GetDateTime(reader.GetOrdinal("CompletedAt")),
                ErrorMessage    = reader.IsDBNull(reader.GetOrdinal("ErrorMessage"))
                                    ? null : reader.GetString(reader.GetOrdinal("ErrorMessage")),
            };
        }

        private static ScrapeJobLogModel MapLog(SqlDataReader reader)
        {
            return new ScrapeJobLogModel
            {
                LogId       = reader.GetInt64(reader.GetOrdinal("LogId")),
                ScrapeJobId = reader.GetInt32(reader.GetOrdinal("ScrapeJobId")),
                Level       = reader.GetString(reader.GetOrdinal("Level")),
                Message     = reader.GetString(reader.GetOrdinal("Message")),
                Timestamp   = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
            };
        }
    }
}
