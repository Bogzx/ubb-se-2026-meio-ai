using Ubb_se_2026_meio_ai.Core.Models;
namespace Ubb_se_2026_meio_ai.Features.MovieTournament.Services
{

    public interface IMovieTournamentRepository
    {
        Task<int> GetTournamentPoolSizeAsync(int userId);
        Task<List<MovieCardModel>> GetTournamentPoolAsync(int userId, int poolSize);
        Task BoostMovieScoreAsync(int userId, int movieId, double scoreBoost);
    }
}
