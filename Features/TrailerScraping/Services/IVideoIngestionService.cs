namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    /// <summary>
    /// Downloads and ingests scraped trailer videos into the local system.
    /// Owner: Andrei
    /// </summary>
    public interface IVideoIngestionService
    {
        Task<string> IngestVideoFromUrlAsync(string trailerUrl, int movieId);
    }
}
