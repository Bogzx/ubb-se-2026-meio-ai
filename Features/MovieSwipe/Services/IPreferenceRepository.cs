using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.MovieSwipe.Services
{
    /// <summary>
    /// Data access for user-movie preference records.
    /// Owner: Bogdan
    /// </summary>
    public interface IPreferenceRepository
    {
        /// <summary>
        /// Returns the preference row for a specific user-movie pair, or null if none exists.
        /// </summary>
        Task<UserMoviePreferenceModel?> GetPreferenceAsync(int userId, int movieId);

        /// <summary>
        /// Inserts a new preference row or updates the existing one (score += delta).
        /// If no row exists, one is created at score 0 before applying the delta.
        /// </summary>
        Task UpsertPreferenceAsync(UserMoviePreferenceModel preference);

        /// <summary>
        /// Returns movies from the external Movie table that the user has NOT yet swiped on.
        /// </summary>
        Task<List<MovieCardModel>> GetUnswipedMoviesAsync(int userId, int count);
    }
}
