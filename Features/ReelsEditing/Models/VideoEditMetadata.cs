namespace ubb_se_2026_meio_ai.Features.ReelsEditing.Models
{
    /// <summary>
    /// Local DTO holding in-progress crop + music edits before saving.
    /// Owner: Beatrice
    /// </summary>
    public class VideoEditMetadata
    {
        public int CropX { get; set; }
        public int CropY { get; set; }
        public int CropWidth { get; set; } = 1920;
        public int CropHeight { get; set; } = 1080;
        public int? SelectedMusicTrackId { get; set; }

        public string ToCropDataJson()
        {
            return $"{{\"x\":{CropX},\"y\":{CropY},\"width\":{CropWidth},\"height\":{CropHeight}}}";
        }
    }
}
