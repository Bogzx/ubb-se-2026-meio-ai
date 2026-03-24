using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{
    /// <summary>
    /// Provides access to the music track library for background audio selection.
    /// Owner: Beatrice
    /// </summary>
    public interface IAudioLibraryService
    {
        Task<IList<MusicTrackModel>> GetAllTracksAsync();
        Task<MusicTrackModel?> GetTrackByIdAsync(int musicTrackId);
    }
}
