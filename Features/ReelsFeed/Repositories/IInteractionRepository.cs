using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.ReelsFeed.Repositories
{
    
    public interface IInteractionRepository
    {
       
        Task InsertInteractionAsync(UserReelInteractionModel interaction);

        Task UpsertInteractionAsync(int userId, int reelId);

 
        Task ToggleLikeAsync(int userId, int reelId);


        Task UpdateViewDataAsync(int userId, int reelId, double watchDurationSec, double watchPercentage);

  
        Task<UserReelInteractionModel?> GetInteractionAsync(int userId, int reelId);


        Task<int> GetLikeCountAsync(int reelId);


        Task<int?> GetReelMovieIdAsync(int reelId);
    }
}
