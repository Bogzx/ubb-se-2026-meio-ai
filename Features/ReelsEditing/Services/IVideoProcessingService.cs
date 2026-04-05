namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.Services
{

    public interface IVideoProcessingService
    {
        Task<string> ApplyCropAsync(string videoPath, string cropDataJson);
        Task<string> MergeAudioAsync(
            string videoPath,
            int musicTrackId,
            double startOffsetSec,
            double musicDurationSec,
            double musicVolumePercent);
    }
}
