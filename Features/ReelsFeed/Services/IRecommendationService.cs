using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.ReelsFeed.Services
{
    /// <summary>
    /// Provides recommended reels for the user's feed.
    /// Owner: Tudor
    /// </summary>
    public interface IRecommendationService
    {
        Task<IList<ReelModel>> GetRecommendedReelsAsync(int userId, int count);
    }
}
