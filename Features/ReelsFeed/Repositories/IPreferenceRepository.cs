namespace ubb_se_2026_meio_ai.Features.ReelsFeed.Repositories
{
   
    public interface IPreferenceRepository
    {
       
        Task BoostPreferenceOnLikeAsync(int userId, int movieId);
    }
}
