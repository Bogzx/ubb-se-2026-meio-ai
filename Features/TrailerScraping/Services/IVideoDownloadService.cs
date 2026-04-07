using System.Threading.Tasks;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    public interface IVideoDownloadService
    {
        string? LastError { get; }
        Task EnsureDependenciesAsync();
        Task<string?> DownloadVideoAsMp4Async(string youtubeUrl, int maxDurationSeconds = 60);
        string GetExpectedFilePath(string videoId);
    }
}