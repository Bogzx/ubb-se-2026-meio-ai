using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Result returned by the YouTube scraper for each video found.
    /// </summary>
    public class ScrapedVideoResult
    {
        private const string YouTubeBaseUrl = "https://www.youtube.com/watch?v=";

        public string VideoId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string ChannelTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string VideoUrl => $"{YouTubeBaseUrl}{VideoId}";
    }

    /// <summary>
    /// Searches YouTube using the Data API v3 to find trailer videos.
    /// Implements <see cref="IWebScraperService"/>.
    /// Owner: Andrei
    /// </summary>
    public class YouTubeScraperService : IYouTubeScraperService
    {
        private const int DefaultMaxResults = 5;
        private const string YouTubeAppName = "MeioAI-TrailerScraper";
        private const string SearchPartSnippet = "snippet";
        private const string SearchTypeVideo = "video";
        private const string FilmAndAnimationCategoryId = "1";

        private readonly string apiKey;

        public YouTubeScraperService(string apiKey)
        {
            this.apiKey = apiKey;
        }

        /// <inheritdoc />
        public async Task<IList<string>> ScrapeTrailerUrlsAsync(string movieTitle)
        {
            var results = await SearchVideosAsync(movieTitle, DefaultMaxResults);
            return results.Select(result => result.VideoUrl).ToList();
        }

        /// <summary>
        /// Performs a YouTube Search.List call and returns rich results.
        /// </summary>
        public async Task<IList<ScrapedVideoResult>> SearchVideosAsync(string query, int maxResults)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = YouTubeAppName
            });

            var searchRequest = youtubeService.Search.List(SearchPartSnippet);
            searchRequest.Q = query;
            searchRequest.MaxResults = maxResults;
            searchRequest.Type = SearchTypeVideo;
            searchRequest.VideoCategoryId = FilmAndAnimationCategoryId;
            searchRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
            searchRequest.SafeSearch = SearchResource.ListRequest.SafeSearchEnum.Moderate;

            var searchResponse = await searchRequest.ExecuteAsync();

            var results = new List<ScrapedVideoResult>();
            if (searchResponse?.Items is null)
            {
                return results;
            }

            foreach (var item in searchResponse.Items)
            {
                if (item.Id?.VideoId is null)
                {
                    continue;
                }

                results.Add(new ScrapedVideoResult
                {
                    VideoId = item.Id.VideoId,
                    Title = item.Snippet?.Title ?? string.Empty,
                    ThumbnailUrl = item.Snippet?.Thumbnails?.High?.Url
                                    ?? item.Snippet?.Thumbnails?.Medium?.Url
                                    ?? item.Snippet?.Thumbnails?.Default__?.Url
                                    ?? string.Empty,
                    ChannelTitle = item.Snippet?.ChannelTitle ?? string.Empty,
                    Description = item.Snippet?.Description ?? string.Empty,
                });
            }

            return results;
        }
    }
}
