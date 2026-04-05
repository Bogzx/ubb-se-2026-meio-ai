namespace Ubb_se_2026_meio_ai.Features.PersonalityMatch.Models
{

    public class MatchResult
    {
        public int MatchedUserId { get; set; }
        public string MatchedUsername { get; set; } = string.Empty;


        public double MatchScore { get; set; }

        public string FacebookAccount { get; set; } = string.Empty;

    
        public bool IsSelfView { get; set; } = false;
    }
}
