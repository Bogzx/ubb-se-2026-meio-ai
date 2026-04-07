using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.PersonalityMatch.Models;

namespace ubb_se_2026_meio_ai.Features.PersonalityMatch.Services
{
    public interface IPersonalityMatchRepository
    {
        Task<Dictionary<int, List<UserMoviePreferenceModel>>> GetAllPreferencesExceptUserAsync(int excludeUserId);

        Task<List<UserMoviePreferenceModel>> GetCurrentUserPreferencesAsync(int userId);

        Task<UserProfileModel?> GetUserProfileAsync(int userId);

        Task<List<int>> GetRandomUserIdsAsync(int excludeUserId, int count);
    
        Task<string> GetUsernameAsync(int userId);
        
        Task<List<MoviePreferenceDisplayModel>> GetTopPreferencesWithTitlesAsync(int userId, int count);
    }
}
