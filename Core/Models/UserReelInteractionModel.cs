namespace ubb_se_2026_meio_ai.Core.Models
{
    public class UserReelInteractionModel
    {
        public long InteractionId { get; set; }
        public int UserId { get; set; }
        public int ReelId { get; set; }
        public bool IsLiked { get; set; }
        public double WatchDurationSec { get; set; }
        public double WatchPercentage { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
