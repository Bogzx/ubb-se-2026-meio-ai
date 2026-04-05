using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    public partial class MusicSelectionDialogViewModel : ObservableObject
    {
        private readonly IAudioLibraryService _audioLibrary;

        [ObservableProperty]
        private ObservableCollection<MusicTrackModel> _availableTracks = new();

        [ObservableProperty]
        private MusicTrackModel? _selectedTrack;

        public MusicSelectionDialogViewModel(IAudioLibraryService audioLibrary)
        {
            _audioLibrary = audioLibrary;
        }

        [RelayCommand]
        private async Task LoadTracksAsync()
        {
            var tracks = await _audioLibrary.GetAllTracksAsync();
            AvailableTracks.Clear();
            foreach (var t in tracks)
                AvailableTracks.Add(t);
        }

        [RelayCommand]
        private void SelectTrack(MusicTrackModel track)
        {
            SelectedTrack = track;
        }
    }
}
