using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.Services
{
    public interface IYouTubeScraperService
    {
        Task<IList<ScrapedVideoResult>> SearchVideosAsync(string query, int maxResults = 5);
    }
}