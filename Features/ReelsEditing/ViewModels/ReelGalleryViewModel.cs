using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    public partial class ReelGalleryViewModel : ObservableObject
    {
        private const int CurrentUserId = 1;
        private const int EmptyReelCount = 0;

        private const string DefaultStatusMessage = "Select a reel to edit.";
        private const string LoadingMessage = "Loading reels...";
        private const string ReelsFoundMessageFormat = "{0} reel(s) found.";
        private const string NoReelsMessage = "No reels uploaded yet. Upload a reel first.";
        private const string ErrorLoadingMessageFormat = "Error loading reels: {0}";

        private readonly IReelRepository reelRepository;

        [ObservableProperty]
        private ObservableCollection<ReelModel> userReels = new ();

        [ObservableProperty]
        private ReelModel? selectedReel;

        [ObservableProperty]
        private string statusMessage = DefaultStatusMessage;

        [ObservableProperty]
        private bool isLoaded;

        public ReelGalleryViewModel(IReelRepository reelRepository)
        {
            this.reelRepository = reelRepository;
        }

        public async Task EnsureLoadedAsync()
        {
            if (!IsLoaded)
            {
                await LoadReelsAsync();
            }
        }

        [RelayCommand]
        private async Task LoadReelsAsync()
        {
            StatusMessage = LoadingMessage;
            try
            {
                var reels = await reelRepository.GetUserReelsAsync(CurrentUserId);
                UserReels.Clear();
                foreach (var reel in reels)
                {
                    UserReels.Add(reel);
                }
                IsLoaded = true;

                StatusMessage = UserReels.Count > EmptyReelCount
                    ? string.Format(ReelsFoundMessageFormat, UserReels.Count)
                    : NoReelsMessage;
            }
            catch (Exception exception)
            {
                StatusMessage = string.Format(ErrorLoadingMessageFormat, exception.Message);
            }
        }
    }
}