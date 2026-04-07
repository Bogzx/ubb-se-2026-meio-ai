using System.Collections.Generic;
using System.Threading.Tasks;
using Ubb_se_2026_meio_ai.Core.Models;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{
    public interface IReelRepository
    {
        Task<IList<ReelModel>> GetUserReelsAsync(int userId);
        Task<ReelModel?> GetReelByIdAsync(int reelId);
        Task<int> UpdateReelEditsAsync(int reelId, string cropDataJson, int? backgroundMusicId, string videoUrl);
        Task DeleteReelAsync(int reelId);
    }
}