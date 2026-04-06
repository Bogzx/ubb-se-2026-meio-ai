using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    public partial class MusicSelectionDialogViewModel : ObservableObject
    {
        private readonly IAudioLibraryService audioLibrary;

        [ObservableProperty]
        private ObservableCollection<MusicTrackModel> availableTracks = new();

        [ObservableProperty]
        private MusicTrackModel? selectedTrack;

        public MusicSelectionDialogViewModel(IAudioLibraryService audioLibrary)
        {
            this.audioLibrary = audioLibrary;
        }

        [RelayCommand]
        private async Task LoadTracksAsync()
        {
            var tracks = await audioLibrary.GetAllTracksAsync();
            AvailableTracks.Clear();

            foreach (var musicTrack in tracks)
            {
                AvailableTracks.Add(musicTrack);
            }
        }

        [RelayCommand]
        private void SelectTrack(MusicTrackModel track)
        {
            SelectedTrack = track;
        }
    }
}