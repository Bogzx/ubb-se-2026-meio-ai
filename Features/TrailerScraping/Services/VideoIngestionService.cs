using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Orchestrates the full scrape flow:
    /// 1. Create a ScrapeJob
    /// 2. Search YouTube via <see cref="YouTubeScraperService"/>
    /// 3. For each result: create Movie (skip duplicates), create Reel (skip duplicates)
    /// 4. Update job status to completed/failed
    /// 
    /// Implements <see cref="IVideoIngestionService"/>.
    /// Owner: Andrei
    /// </summary>
    public class VideoIngestionService : IVideoIngestionService
    {
        private readonly YouTubeScraperService _scraper;
        private readonly IScrapeJobRepository _repository;

        public VideoIngestionService(YouTubeScraperService scraper, IScrapeJobRepository repository)
        {
            _scraper = scraper;
            _repository = repository;
        }

        /// <inheritdoc />
        public async Task<string> IngestVideoFromUrlAsync(string trailerUrl, int movieId)
        {
            bool exists = await _repository.ReelExistsByVideoUrlAsync(trailerUrl);
            if (exists)
            {
                return string.Empty; // duplicate
            }

            var reel = new ReelModel
            {
                MovieId = movieId,
                CreatorUserId = 0, // system/scraped
                VideoUrl = trailerUrl,
                Title = "Scraped Trailer",
                Caption = string.Empty,
                ThumbnailUrl = string.Empty,
                Source = "scraped",
                CreatedAt = DateTime.UtcNow,
            };

            int reelId = await _repository.InsertScrapedReelAsync(reel);
            return reelId.ToString();
        }

        /// <summary>
        /// Runs a full scrape job: search YouTube, create movies, create reels, log everything.
        /// </summary>
        /// <param name="searchQuery">The YouTube search query.</param>
        /// <param name="maxResults">Maximum number of results to fetch.</param>
        /// <param name="onLogEntry">Optional callback invoked for each log entry (for live UI updates).</param>
        /// <returns>The completed <see cref="ScrapeJobModel"/>.</returns>
        public async Task<ScrapeJobModel> RunScrapeJobAsync(
            string searchQuery,
            int maxResults,
            Func<ScrapeJobLogModel, Task>? onLogEntry = null)
        {
            // 1. Create job record
            var job = new ScrapeJobModel
            {
                SearchQuery = searchQuery,
                MaxResults = maxResults,
                Status = "running",
                StartedAt = DateTime.UtcNow,
            };
            job.ScrapeJobId = await _repository.CreateJobAsync(job);

            async Task LogAsync(string level, string message)
            {
                var logEntry = new ScrapeJobLogModel
                {
                    ScrapeJobId = job.ScrapeJobId,
                    Level = level,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                };
                await _repository.AddLogEntryAsync(logEntry);
                if (onLogEntry is not null)
                {
                    await onLogEntry(logEntry);
                }
            }

            try
            {
                await LogAsync("Info", $"Starting scrape job: \"{searchQuery}\" (max {maxResults} results)");

                // 2. Search YouTube
                IList<ScrapedVideoResult> results = await _scraper.SearchVideosAsync(searchQuery, maxResults);
                await LogAsync("Info", $"YouTube returned {results.Count} result(s)");

                int moviesCreated = 0;
                int reelsCreated = 0;

                // 3. Process each result
                foreach (var video in results)
                {
                    try
                    {
                        // --- Movie ---
                        int movieId;
                        int? existingMovieId = await _repository.FindMovieByTitleAsync(video.Title);
                        if (existingMovieId.HasValue)
                        {
                            movieId = existingMovieId.Value;
                            await LogAsync("Info", $"Movie already exists: \"{video.Title}\" (ID {movieId})");
                        }
                        else
                        {
                            movieId = await _repository.InsertMovieAsync(
                                video.Title,
                                video.ThumbnailUrl,
                                string.Empty,          // genre not available from search
                                video.Description,
                                0                       // release year not available from search
                            );
                            moviesCreated++;
                            await LogAsync("Info", $"Created movie: \"{video.Title}\" (ID {movieId})");
                        }

                        // --- Reel ---
                        bool reelExists = await _repository.ReelExistsByVideoUrlAsync(video.VideoUrl);
                        if (reelExists)
                        {
                            await LogAsync("Warn", $"Reel already exists for URL: {video.VideoUrl}");
                            continue;
                        }

                        var reel = new ReelModel
                        {
                            MovieId = movieId,
                            CreatorUserId = 0, // system/scraped sentinel
                            VideoUrl = video.VideoUrl,
                            ThumbnailUrl = video.ThumbnailUrl,
                            Title = video.Title,
                            Caption = $"Scraped from YouTube — {video.ChannelTitle}",
                            Source = "scraped",
                            CreatedAt = DateTime.UtcNow,
                        };

                        int reelId = await _repository.InsertScrapedReelAsync(reel);
                        reelsCreated++;
                        await LogAsync("Info", $"Created reel (ID {reelId}) for \"{video.Title}\"");
                    }
                    catch (Exception ex)
                    {
                        await LogAsync("Error", $"Failed to process \"{video.Title}\": {ex.Message}");
                    }
                }

                // 4. Complete job
                job.MoviesFound = moviesCreated;
                job.ReelsCreated = reelsCreated;
                job.Status = "completed";
                job.CompletedAt = DateTime.UtcNow;
                await _repository.UpdateJobAsync(job);

                await LogAsync("Info", $"Job completed — {moviesCreated} movie(s), {reelsCreated} reel(s) created");
            }
            catch (Exception ex)
            {
                job.Status = "failed";
                job.CompletedAt = DateTime.UtcNow;
                job.ErrorMessage = ex.Message;
                await _repository.UpdateJobAsync(job);

                try { await LogAsync("Error", $"Job failed: {ex.Message}"); }
                catch { /* best effort logging */ }
            }

            return job;
        }
    }
}
