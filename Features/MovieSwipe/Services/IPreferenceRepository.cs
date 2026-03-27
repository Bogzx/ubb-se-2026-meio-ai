using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.MovieSwipe.Services
{

    public interface IPreferenceRepository
    {
       
        Task<UserMoviePreferenceModel?> GetPreferenceAsync(int userId, int movieId);

        
        Task UpsertPreferenceAsync(UserMoviePreferenceModel preference);

   
        Task<Dictionary<int, List<UserMoviePreferenceModel>>> GetAllPreferencesExceptUserAsync(int excludeUserId);


        Task<List<int>> GetUnswipedMovieIdsAsync(int userId);

  
        Task<List<MovieCardModel>> GetMovieFeedAsync(int userId, int count);
    }
}
