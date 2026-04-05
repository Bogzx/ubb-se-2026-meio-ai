namespace Ubb_se_2026_meio_ai.Core.Models
{
    public class UserMoviePreferenceModel
    {
        public int UserMoviePreferenceId { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public double Score { get; set; }
        public DateTime LastModified { get; set; }
        public int? ChangeFromPreviousValue { get; set; }
    }
}
