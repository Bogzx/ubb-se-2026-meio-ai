using ubb_se_2026_meio_ai.Core.Models;

namespace ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{

    public interface IAudioLibraryService
    {
        Task<IList<MusicTrackModel>> GetAllTracksAsync();
        Task<MusicTrackModel?> GetTrackByIdAsync(int musicTrackId);
    }
}
