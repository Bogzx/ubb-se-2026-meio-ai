namespace ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Scrapes trailer metadata and URLs from external sources.
    /// Owner: Andrei
    /// </summary>
    public interface IWebScraperService
    {
        Task<IList<string>> ScrapeTrailerUrlsAsync(string movieTitle);
    }
}
