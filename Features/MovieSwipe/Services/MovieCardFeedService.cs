using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.MovieSwipe.Services
{
    public interface IMovieCardFeedService
    {
        Task<List<MovieCardModel>> FetchMovieFeedAsync(int userId, int count);
    }

    public class MovieCardFeedService : IMovieCardFeedService
    {
        private readonly IPreferenceRepository _repository;

        public MovieCardFeedService(IPreferenceRepository repository)
        {
            _repository = repository;
        }

        public Task<List<MovieCardModel>> FetchMovieFeedAsync(int userId, int count)
        {
            return _repository.GetMovieFeedAsync(userId, count);
        }
    }
}
