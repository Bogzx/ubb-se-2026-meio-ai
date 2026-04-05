using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.MovieSwipe.Services
{

    public class SwipeService : ISwipeService
    {
       
        public const double LikeDelta = 1.0;

       
        public const double SkipDelta = -0.5;

        private readonly IPreferenceRepository _preferenceRepository;

        public SwipeService(IPreferenceRepository preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }


        public async Task UpdatePreferenceScoreAsync(int userId, int movieId, bool isLiked)
        {
            double delta = isLiked ? LikeDelta : SkipDelta;

            var preference = new UserMoviePreferenceModel
            {
                UserId = userId,
                MovieId = movieId,
                Score = delta,
                LastModified = DateTime.UtcNow,
                ChangeFromPreviousValue = isLiked ? 1 : -1
            };

            await _preferenceRepository.UpsertPreferenceAsync(preference);
        }

  
        public async Task<List<MovieCardModel>> GetMovieFeedAsync(int userId, int count)
        {
            return await _preferenceRepository.GetMovieFeedAsync(userId, count);
        }
    }
}
