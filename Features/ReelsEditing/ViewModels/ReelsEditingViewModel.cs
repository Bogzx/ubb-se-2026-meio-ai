using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels
{
    public partial class ReelsEditingViewModel : ObservableObject
    {
        private const int BaseVideoWidth = 1920;
        private const int BaseVideoHeight = 1080;
        private const double DefaultMusicDurationSeconds = 30.0;
        private const double DefaultMusicVolume = 80.0;
        private const double MinMusicDurationSeconds = 5.0;
        private const double MaxMusicDurationSeconds = 120.0;
        private const double MinMusicVolume = 0.0;
        private const double MaxMusicVolume = 100.0;
        private const double MinCropMargin = 0.0;
        private const double MaxCropMargin = 45.0;
        private const double PercentageDivisor = 100.0;
        private const double FullPercentage = 1.0;
        private const double MaxMusicStartTime = 300.0;
        private const double EmptyValue = 0.0;
        private const int EmptyRowsAffected = 0;

        private const string OptionCrop = "Crop";
        private const string OptionMusic = "Music";
        private const string JsonKeyX = "x";
        private const string JsonKeyY = "y";
        private const string JsonKeyWidth = "width";
        private const string JsonKeyHeight = "height";
        private const string JsonKeyMusicStartTime = "musicStartTime";
        private const string JsonKeyMusicDuration = "musicDuration";
        private const string JsonKeyMusicVolume = "musicVolume";

        private const string StatusMusicSelectedFormat = "Music selected: {0}";
        private const string StatusLoadMusicFailedFormat = "Failed to load music: {0}";
        private const string StatusSavingCrop = "Saving crop...";
        private const string StatusSavingMusic = "Saving music...";
        private const string StatusDeletingReel = "Deleting reel...";
        private const string StatusReelDeleted = "Reel deleted.";
        private const string StatusCropUpdatedFormat = "Crop dimensions updated successfully: X={0}, Y={1}, W={2}, H={3}.";
        private const string StatusSaveFailedFormat = "Save failed: {0}";
        private const string StatusDeleteFailedFormat = "Delete failed: {0}";
        private const string ErrorReelNotFoundFormat = "No reel found with ReelId={0}.";
        private const string ErrorCropPersistFailed = "Crop edits were not persisted correctly.";
        private const string ErrorMusicPersistFailed = "Music edits were not persisted correctly.";

        private readonly ReelRepository reelRepository;
        private readonly IVideoProcessingService videoProcessing;
        private readonly IAudioLibraryService audioLibrary;

        [ObservableProperty]
        private ReelModel? selectedReel;

        [ObservableProperty]
        private VideoEditMetadata currentEdits = new ();

        [ObservableProperty]
        private MusicTrackModel? selectedMusicTrack;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isStatusSuccess = true;

        [ObservableProperty]
        private bool isSaving;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private string selectedEditOption = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MusicTrackModel> musicTracks = new ();

        [ObservableProperty]
        private bool isMusicChosen;

        [ObservableProperty]
        private double cropMarginLeft;

        [ObservableProperty]
        private double cropMarginTop;

        [ObservableProperty]
        private double cropMarginRight;

        [ObservableProperty]
        private double cropMarginBottom;

        [ObservableProperty]
        private double musicStartTime;

        [ObservableProperty]
        private double musicDuration = DefaultMusicDurationSeconds;

        [ObservableProperty]
        private double musicVolume = DefaultMusicVolume;

        public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);

        public event Action? CropModeEntered;
        public event Action? CropModeExited;
        public event Action? CropSaveStarted;
        public event Action<string>? CropVideoUpdated;

        public ReelsEditingViewModel(
            ReelRepository reelRepository,
            IVideoProcessingService videoProcessing,
            IAudioLibraryService audioLibrary)
        {
            this.reelRepository = reelRepository;
            this.videoProcessing = videoProcessing;
            this.audioLibrary = audioLibrary;
        }

        [RelayCommand]
        private void SelectEditOption(string option)
        {
            if (SelectedEditOption == option)
            {
                if (SelectedEditOption == OptionCrop)
                {
                    CropModeExited?.Invoke();
                }
                SelectedEditOption = string.Empty;
                return;
            }

            if (SelectedEditOption == OptionCrop)
            {
                CropModeExited?.Invoke();
            }
            SelectedEditOption = option;

            if (option == OptionCrop)
            {
                CropModeEntered?.Invoke();
            }

            if (option == OptionMusic)
            {
                _ = LoadMusicTracksAsync();
            }
        }

        public async Task LoadReelAsync(ReelModel reel)
        {
            var freshReelData = await reelRepository.GetReelByIdAsync(reel.ReelId);
            if (freshReelData != null)
            {
                reel.VideoUrl = freshReelData.VideoUrl;
                reel.CropDataJson = freshReelData.CropDataJson;
                reel.BackgroundMusicId = freshReelData.BackgroundMusicId;
                reel.LastEditedAt = freshReelData.LastEditedAt;
            }

            SelectedReel = reel;
            CurrentEdits = new VideoEditMetadata();
            SelectedMusicTrack = null;
            IsMusicChosen = false;
            IsEditing = true;
            SelectedEditOption = string.Empty;
            CropMarginLeft = EmptyValue;
            CropMarginTop = EmptyValue;
            CropMarginRight = EmptyValue;
            CropMarginBottom = EmptyValue;
            MusicStartTime = EmptyValue;
            MusicDuration = DefaultMusicDurationSeconds;
            MusicVolume = DefaultMusicVolume;
            StatusMessage = string.Empty;
            IsStatusSuccess = true;

            LoadPersistedEditData(reel.CropDataJson, reel.BackgroundMusicId);

            if (reel.BackgroundMusicId.HasValue)
            {
                try
                {
                    var track = await audioLibrary.GetTrackByIdAsync(reel.BackgroundMusicId.Value);
                    if (track != null)
                    {
                        SelectedMusicTrack = track;
                        NormalizeMusicTimingForSelectedTrack();
                    }
                }
                catch
                {
                    /* Non-fatal */
                }
            }
        }

        [RelayCommand]
        private void GoBack()
        {
            if (SelectedEditOption == OptionCrop)
            {
                CropModeExited?.Invoke();
            }
            SelectedReel = null;
            IsEditing = false;
            SelectedEditOption = string.Empty;
            StatusMessage = string.Empty;
            IsStatusSuccess = true;
        }

        public void ApplyMusicSelection(MusicTrackModel track)
        {
            SelectedMusicTrack = track;
            CurrentEdits.SelectedMusicTrackId = track.MusicTrackId;
            IsMusicChosen = true;
            MusicStartTime = EmptyValue;

            double reelDuration = SelectedReel?.FeatureDurationSeconds ?? DefaultMusicDurationSeconds;
            MusicDuration = Math.Clamp(reelDuration, MinMusicDurationSeconds, MaxMusicDurationSeconds);

            NormalizeMusicTimingForSelectedTrack();
            IsStatusSuccess = true;
            StatusMessage = string.Format(StatusMusicSelectedFormat, track.TrackName);
        }

        private async Task LoadMusicTracksAsync()
        {
            try
            {
                var tracks = await audioLibrary.GetAllTracksAsync();
                MusicTracks.Clear();
                foreach (var musicTrack in tracks)
                {
                    MusicTracks.Add(musicTrack);
                }
            }
            catch (Exception exception)
            {
                IsStatusSuccess = false;
                StatusMessage = string.Format(StatusLoadMusicFailedFormat, exception.Message);
            }
        }

        [RelayCommand]
        private async Task SaveCropAsync()
        {
            if (SelectedReel == null)
            {
                return;
            }
            IsSaving = true;
            StatusMessage = StatusSavingCrop;
            IsStatusSuccess = true;
            try
            {
                CropSaveStarted?.Invoke();

                CurrentEdits.CropXCoordinate = (int)((CropMarginLeft / PercentageDivisor) * BaseVideoWidth);
                CurrentEdits.CropYCoordinate = (int)((CropMarginTop / PercentageDivisor) * BaseVideoHeight);
                CurrentEdits.CropWidth = (int)((FullPercentage - ((CropMarginLeft + CropMarginRight) / PercentageDivisor)) * BaseVideoWidth);
                CurrentEdits.CropHeight = (int)((FullPercentage - ((CropMarginTop + CropMarginBottom) / PercentageDivisor)) * BaseVideoHeight);

                string cropJson = CurrentEdits.ToCropDataJson();
                string processedVideoPath = await videoProcessing.ApplyCropAsync(SelectedReel.VideoUrl, cropJson);

                int rowsAffected = await reelRepository.UpdateReelEditsAsync(
                    SelectedReel.ReelId,
                    cropJson,
                    CurrentEdits.SelectedMusicTrackId,
                    processedVideoPath);

                if (rowsAffected == EmptyRowsAffected)
                {
                    throw new InvalidOperationException(string.Format(ErrorReelNotFoundFormat, SelectedReel.ReelId));
                }

                var persistedReel = await reelRepository.GetReelByIdAsync(SelectedReel.ReelId);
                if (persistedReel == null || persistedReel.CropDataJson != cropJson)
                {
                    throw new InvalidOperationException(ErrorCropPersistFailed);
                }

                SelectedReel.VideoUrl = persistedReel.VideoUrl;
                SelectedReel.CropDataJson = persistedReel.CropDataJson;
                SelectedReel.LastEditedAt = persistedReel.LastEditedAt;
                CropVideoUpdated?.Invoke(SelectedReel.VideoUrl);

                StatusMessage = string.Format(
                    StatusCropUpdatedFormat,
                    CurrentEdits.CropXCoordinate,
                    CurrentEdits.CropYCoordinate,
                    CurrentEdits.CropWidth,
                    CurrentEdits.CropHeight);
            }
            catch (Exception exception)
            {
                IsStatusSuccess = false;
                StatusMessage = string.Format(StatusSaveFailedFormat, exception.Message);
                CropVideoUpdated?.Invoke(SelectedReel.VideoUrl);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task SaveMusicAsync()
        {
            if (SelectedReel == null || SelectedMusicTrack == null)
            {
                return;
            }
            IsSaving = true;
            StatusMessage = StatusSavingMusic;
            IsStatusSuccess = true;
            try
            {
                CropSaveStarted?.Invoke();

                CurrentEdits.SelectedMusicTrackId = SelectedMusicTrack.MusicTrackId;
                NormalizeMusicTimingForSelectedTrack();
                CurrentEdits.MusicStartTime = MusicStartTime;
                CurrentEdits.MusicDuration = MusicDuration;
                CurrentEdits.MusicVolume = MusicVolume;

                string processedVideoPath = await videoProcessing.MergeAudioAsync(
                    SelectedReel.VideoUrl,
                    SelectedMusicTrack.MusicTrackId,
                    MusicStartTime,
                    MusicDuration,
                    MusicVolume);

                int rowsAffected = await reelRepository.UpdateReelEditsAsync(
                    SelectedReel.ReelId,
                    CurrentEdits.ToCropDataJson(),
                    SelectedMusicTrack.MusicTrackId,
                    processedVideoPath);

                if (rowsAffected == EmptyRowsAffected)
                {
                    throw new InvalidOperationException(string.Format(ErrorReelNotFoundFormat, SelectedReel.ReelId));
                }

                var persistedReel = await reelRepository.GetReelByIdAsync(SelectedReel.ReelId);
                if (persistedReel == null || persistedReel.BackgroundMusicId != SelectedMusicTrack.MusicTrackId)
                {
                    throw new InvalidOperationException(ErrorMusicPersistFailed);
                }

                SelectedReel.VideoUrl = persistedReel.VideoUrl;
                SelectedReel.CropDataJson = persistedReel.CropDataJson;
                SelectedReel.BackgroundMusicId = persistedReel.BackgroundMusicId;
                SelectedReel.LastEditedAt = persistedReel.LastEditedAt;
                CropVideoUpdated?.Invoke(SelectedReel.VideoUrl);

                StatusMessage = string.Format(StatusMusicSelectedFormat, SelectedMusicTrack.TrackName);
            }
            catch (Exception exception)
            {
                IsStatusSuccess = false;
                StatusMessage = string.Format(StatusSaveFailedFormat, exception.Message);
                CropVideoUpdated?.Invoke(SelectedReel.VideoUrl);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private async Task DeleteReelAsync()
        {
            if (SelectedReel == null)
            {
                return;
            }
            try
            {
                StatusMessage = StatusDeletingReel;
                await reelRepository.DeleteReelAsync(SelectedReel.ReelId);
                StatusMessage = StatusReelDeleted;
                GoBack();
            }
            catch (Exception exception)
            {
                IsStatusSuccess = false;
                StatusMessage = string.Format(StatusDeleteFailedFormat, exception.Message);
            }
        }

        partial void OnStatusMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasStatusMessage));
        }

        private void LoadPersistedEditData(string? cropDataJson, int? backgroundMusicId)
        {
            CurrentEdits.SelectedMusicTrackId = backgroundMusicId;
            if (backgroundMusicId.HasValue)
            {
                IsMusicChosen = true;
            }
            if (string.IsNullOrWhiteSpace(cropDataJson))
            {
                return;
            }
            try
            {
                using var jsonDocument = JsonDocument.Parse(cropDataJson);
                var rootElement = jsonDocument.RootElement;

                CurrentEdits.CropXCoordinate = ReadInt(rootElement, JsonKeyX, (int)EmptyValue);
                CurrentEdits.CropYCoordinate = ReadInt(rootElement, JsonKeyY, (int)EmptyValue);
                CurrentEdits.CropWidth = ReadInt(rootElement, JsonKeyWidth, BaseVideoWidth);
                CurrentEdits.CropHeight = ReadInt(rootElement, JsonKeyHeight, BaseVideoHeight);
                CurrentEdits.MusicStartTime = Math.Max(EmptyValue, ReadDouble(rootElement, JsonKeyMusicStartTime, EmptyValue));
                CurrentEdits.MusicDuration = Math.Clamp(ReadDouble(rootElement, JsonKeyMusicDuration, DefaultMusicDurationSeconds), MinMusicDurationSeconds, MaxMusicDurationSeconds);
                CurrentEdits.MusicVolume = Math.Clamp(ReadDouble(rootElement, JsonKeyMusicVolume, DefaultMusicVolume), MinMusicVolume, MaxMusicVolume);

                CropMarginLeft = Math.Clamp((CurrentEdits.CropXCoordinate / (double)BaseVideoWidth) * PercentageDivisor, MinCropMargin, MaxCropMargin);
                CropMarginTop = Math.Clamp((CurrentEdits.CropYCoordinate / (double)BaseVideoHeight) * PercentageDivisor, MinCropMargin, MaxCropMargin);
                CropMarginRight = Math.Clamp(((BaseVideoWidth - (CurrentEdits.CropXCoordinate + CurrentEdits.CropWidth)) / (double)BaseVideoWidth) * PercentageDivisor, MinCropMargin, MaxCropMargin);
                CropMarginBottom = Math.Clamp(((BaseVideoHeight - (CurrentEdits.CropYCoordinate + CurrentEdits.CropHeight)) / (double)BaseVideoHeight) * PercentageDivisor, MinCropMargin, MaxCropMargin);

                MusicStartTime = CurrentEdits.MusicStartTime;
                MusicDuration = CurrentEdits.MusicDuration;
                MusicVolume = CurrentEdits.MusicVolume;
            }
            catch
            {
                // Keep defaults if previously stored JSON is malformed.
            }
        }

        private void NormalizeMusicTimingForSelectedTrack()
        {
            MusicStartTime = Math.Clamp(MusicStartTime, EmptyValue, MaxMusicStartTime);
            MusicDuration = Math.Clamp(MusicDuration, MinMusicDurationSeconds, MaxMusicDurationSeconds);
            MusicVolume = Math.Clamp(MusicVolume, MinMusicVolume, MaxMusicVolume);
        }

        private static int ReadInt(JsonElement rootElement, string propertyName, int fallbackValue)
        {
            if (rootElement.TryGetProperty(propertyName, out var jsonValue))
            {
                if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetInt32(out var parsedInteger))
                {
                    return parsedInteger;
                }
                if (jsonValue.ValueKind == JsonValueKind.String && int.TryParse(jsonValue.GetString(), out var parsedFromString))
                {
                    return parsedFromString;
                }
            }

            return fallbackValue;
        }

        private static double ReadDouble(JsonElement rootElement, string propertyName, double fallbackValue)
        {
            if (rootElement.TryGetProperty(propertyName, out var jsonValue))
            {
                if (jsonValue.ValueKind == JsonValueKind.Number && jsonValue.TryGetDouble(out var parsedDouble))
                {
                    return parsedDouble;
                }
                if (jsonValue.ValueKind == JsonValueKind.String && double.TryParse(jsonValue.GetString(), out var parsedFromString))
                {
                    return parsedFromString;
                }
            }

            return fallbackValue;
        }
    }
}