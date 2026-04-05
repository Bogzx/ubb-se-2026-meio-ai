using Ubb_se_2026_meio_ai.Features.PersonalityMatch.Models;

namespace Ubb_se_2026_meio_ai.Features.PersonalityMatch.Services
{

    public interface IPersonalityMatchingService
    {

        Task<List<MatchResult>> GetTopMatchesAsync(int userId, int count);

        Task<List<MatchResult>> GetRandomUsersAsync(int userId, int count);
    }
}
