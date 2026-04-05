using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.ReelsFeed.Services
{
    /// <summary>
    /// Manages the user's engagement profile (aggregated watch stats).
    /// Owner: Tudor
    /// </summary>
    public interface IEngagementProfileService
    {
        Task<UserProfileModel?> GetProfileAsync(int userId);
        Task RefreshProfileAsync(int userId);
    }
}
