namespace Ubb_se_2026_meio_ai.Core.Models
{
    public class UserProfileModel
    {
        public int UserProfileId { get; set; }
        public int UserId { get; set; }
        public int TotalLikes { get; set; }
        public long TotalWatchTimeSec { get; set; }
        public double AvgWatchTimeSec { get; set; }
        public int TotalClipsViewed { get; set; }
        public double LikeToViewRatio { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
