using System.Threading.Tasks;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsFeed.Repositories;

namespace Ubb_se_2026_meio_ai.Features.ReelsFeed.Services
{
    /// <summary>
    /// Orchestrates engagement profile operations and delegates all data access
    /// concerns to <see cref="IProfileRepository"/>.
    /// Owner: Tudor
    /// </summary>
    public class EngagementProfileService : IEngagementProfileService
    {
        private readonly IProfileRepository profileRepository;

        public EngagementProfileService(IProfileRepository profileRepository)
        {
            this.profileRepository = profileRepository;
        }

        public async Task<UserProfileModel?> GetProfileAsync(int userId)
        {
            return await profileRepository.GetProfileAsync(userId);
        }

        /// <summary>
        /// Rebuilds and persists the engagement profile for a user.
        /// </summary>
        public async Task RefreshProfileAsync(int userId)
        {
            // Step 1: aggregate raw interaction data in repository
            var profile = await profileRepository.BuildProfileFromInteractionsAsync(userId);

            // Step 2: persist via repository
            await profileRepository.UpsertProfileAsync(profile);
        }
    }
}