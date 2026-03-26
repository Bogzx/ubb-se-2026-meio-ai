using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.ReelsEditing.Models;
using ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    /// <summary>
    /// ViewModel for the main Reel Editor workspace.
    /// Owner: Beatrice
    /// </summary>
    public partial class ReelsEditingViewModel : ObservableObject
    {
        private readonly ReelRepository _reelRepository;
        private readonly IVideoProcessingService _videoProcessing;

        [ObservableProperty]
        private ReelModel? _selectedReel;

        [ObservableProperty]
        private VideoEditMetadata _currentEdits = new();

        [ObservableProperty]
        private MusicTrackModel? _selectedMusicTrack;

        [ObservableProperty]
        private string _statusMessage = "Select a reel to edit.";

        [ObservableProperty]
        private bool _isSaving;

        [ObservableProperty]
        private bool _isEditing;

        public ReelsEditingViewModel(
            ReelRepository reelRepository,
            IVideoProcessingService videoProcessing)
        {
            _reelRepository = reelRepository;
            _videoProcessing = videoProcessing;
        }

        public void LoadReel(ReelModel reel)
        {
            SelectedReel = reel;
            // Pre-populate crop if saved previously
            CurrentEdits = new VideoEditMetadata();
            SelectedMusicTrack = null;
            IsEditing = true;
            StatusMessage = $"Editing: {reel.Title}";
        }

        [RelayCommand]
        private void GoBack()
        {
            SelectedReel = null;
            IsEditing = false;
            StatusMessage = "Select a reel to edit.";
        }

        public void ApplyMusicSelection(MusicTrackModel track)
        {
            SelectedMusicTrack = track;
            CurrentEdits.SelectedMusicTrackId = track.MusicTrackId;
            StatusMessage = $"Music selected: {track.TrackName}";
        }

        [RelayCommand]
        private async Task SaveEditsAsync()
        {
            if (SelectedReel == null)
            {
                StatusMessage = "No reel selected.";
                return;
            }

            IsSaving = true;
            StatusMessage = "Saving edits...";
            try
            {
                string cropJson = CurrentEdits.ToCropDataJson();
                int? musicId = CurrentEdits.SelectedMusicTrackId;

                // Apply processing (stub — returns original path)
                await _videoProcessing.ApplyCropAsync(SelectedReel.VideoUrl, cropJson);
                if (musicId.HasValue)
                    await _videoProcessing.MergeAudioAsync(SelectedReel.VideoUrl, musicId.Value, 0);

                // Persist to DB
                await _reelRepository.UpdateReelEditsAsync(SelectedReel.ReelId, cropJson, musicId);

                StatusMessage = "Edits saved successfully!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Save failed: {ex.Message}";
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void ResetEdits()
        {
            CurrentEdits = new VideoEditMetadata();
            SelectedMusicTrack = null;
            StatusMessage = "Edits reset.";
        }
    }
}
