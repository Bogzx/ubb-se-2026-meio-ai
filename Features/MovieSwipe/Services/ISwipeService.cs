using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.MovieSwipe.Services
{

    public interface ISwipeService
    {

        Task UpdatePreferenceScoreAsync(int userId, int movieId, bool isLiked);


        Task<List<MovieCardModel>> GetMovieFeedAsync(int userId, int count);
    }
}
