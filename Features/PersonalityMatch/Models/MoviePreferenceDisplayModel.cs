namespace ubb_se_2026_meio_ai.Features.PersonalityMatch.Models
{

    public class MoviePreferenceDisplayModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public double Score { get; set; }

        public bool IsBestMovie { get; set; }
    }
}
