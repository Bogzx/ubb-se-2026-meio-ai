using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.ReelsFeed.Repositories;

namespace ubb_se_2026_meio_ai.Features.ReelsFeed.Services
{
    /// <summary>
    /// Computes the user's engagement profile by aggregating raw interaction data,
    /// then delegates persistence to <see cref="IProfileRepository"/>.
    /// Owner: Tudor.
    /// </summary>
    public class EngagementProfileService : IEngagementProfileService
    {
        private const int EngagementStats_TotalLikes_Index = 0;
        private const int EngagementStats_TotalWatchTimeSec_Index = 1;
        private const int EngagementStats_AverageWatchTimeSec_Index = 2;
        private const int EngagementStats_TotalClipsViewed_Index = 3;

        private readonly IProfileRepository _profileRepository;

        public EngagementProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        /// <inheritdoc />
        public async Task<UserProfileModel?> GetProfileAsync(int userId)
        {
            return await this._profileRepository.GetProfileAsync(userId);
        }

        /// <inheritdoc />
        public async Task RefreshProfileAsync(int userId)
        {
            // Step 1: aggregate raw interaction data in repository
            var refreshedProfile = await _profileRepository.BuildProfileFromInteractionsAsync(userId);

            // Step 2: persist via repository
            await this._profileRepository.UpsertProfileAsync(refreshedProfile);
        }
    }
}
