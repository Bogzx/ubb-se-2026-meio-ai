namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.Models
{
    public class VideoEditMetadata
    {
        private const int DefaultCropWidth = 1920;
        private const int DefaultCropHeight = 1080;
        private const double DefaultMusicDurationSeconds = 30.0;
        private const double DefaultMusicVolumePercentage = 80.0;

        public int CropXCoordinate { get; set; }
        public int CropYCoordinate { get; set; }
        public int CropWidth { get; set; } = DefaultCropWidth;
        public int CropHeight { get; set; } = DefaultCropHeight;
        public int? SelectedMusicTrackId { get; set; }

        // Music parameters
        public double MusicStartTime { get; set; }
        public double MusicDuration { get; set; } = DefaultMusicDurationSeconds;
        public double MusicVolume { get; set; } = DefaultMusicVolumePercentage;

        public string ToCropDataJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                // Keeping the JSON keys as x and y to prevent breaking the database parser
                x = CropXCoordinate,
                y = CropYCoordinate,
                width = CropWidth,
                height = CropHeight,
                musicStartTime = MusicStartTime,
                musicDuration = MusicDuration,
                musicVolume = MusicVolume
            });
        }
    }
}