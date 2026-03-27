# Class Diagram — Beatrice (Reels Editing)

```mermaid
classDiagram
    direction TB

    %% ── Models & DTOs ──
    class VideoEditMetadata {
        +int CropX
        +int CropY
        +int CropWidth
        +int CropHeight
        +int? SelectedMusicTrackId
        +double MusicStartTime
        +double MusicDuration
        +double MusicVolume
        +ToCropDataJson() string
    }

    class ReelModel {
        <<Core Model>>
    }

    class MusicTrackModel {
        <<Core Model>>
    }

    %% ── Repositories ──
    class ReelRepository {
        -ISqlConnectionFactory _db
        +ReelRepository(ISqlConnectionFactory db)
        +GetUserReelsAsync(int userId) Task~IList~ReelModel~~
        +UpdateReelEditsAsync(int reelId, string cropDataJson, int? musicId, string? videoUrl) Task~int~
        +GetReelByIdAsync(int reelId) Task~ReelModel?~
        +DeleteReelAsync(int reelId) Task
    }

    %% ── Services ──
    class IAudioLibraryService {
        <<interface>>
        +GetAllTracksAsync() Task~IList~MusicTrackModel~~
        +GetTrackByIdAsync(int musicTrackId) Task~MusicTrackModel?~
    }

    class AudioLibraryService {
        -ISqlConnectionFactory _db
        +AudioLibraryService(ISqlConnectionFactory db)
        +GetAllTracksAsync() Task~IList~MusicTrackModel~~
        +GetTrackByIdAsync(int musicTrackId) Task~MusicTrackModel?~
    }

    class IVideoProcessingService {
        <<interface>>
        +ApplyCropAsync(string videoPath, string cropDataJson) Task~string~
        +MergeAudioAsync(string videoPath, int musicTrackId, double startOffsetSec, double musicDurationSec, double musicVolumePercent) Task~string~
    }

    class VideoProcessingService {
        -IAudioLibraryService _audioLibrary
        +VideoProcessingService(IAudioLibraryService audioLibrary)
        +ApplyCropAsync(string videoPath, string cropDataJson) Task~string~
        +MergeAudioAsync(string videoPath, int musicTrackId, double startOffsetSec, double musicDurationSec, double musicVolumePercent) Task~string~
    }

    %% ── ViewModels ──
    class ReelsEditingViewModel {
        -ReelRepository _reelRepository
        -IVideoProcessingService _videoProcessing
        -IAudioLibraryService _audioLibrary
        +ReelModel? SelectedReel
        +VideoEditMetadata CurrentEdits
        +MusicTrackModel? SelectedMusicTrack
        +string StatusMessage
        +bool IsStatusSuccess
        +bool IsSaving
        +bool IsEditing
        +string SelectedEditOption
        +double CropMarginLeft
        +double CropMarginTop
        +double CropMarginRight
        +double CropMarginBottom
        +double MusicStartTime
        +double MusicDuration
        +double MusicVolume
        +ObservableCollection~MusicTrackModel~ MusicTracks
        +bool IsMusicChosen
        +bool HasStatusMessage
        +event Action? CropModeEntered
        +event Action? CropModeExited
        +event Action? CropSaveStarted
        +event Action~string~? CropVideoUpdated
        +SelectEditOptionCommand
        +GoBackCommand
        +SaveCropCommand
        +SaveMusicCommand
        +DeleteReelCommand
        +LoadReelAsync(ReelModel reel) Task
        +ApplyMusicSelection(MusicTrackModel track) void
    }

    class ReelGalleryViewModel {
        -ReelRepository _reelRepository
        -int CurrentUserId
        +ObservableCollection~ReelModel~ UserReels
        +ReelModel? SelectedReel
        +string StatusMessage
        +bool IsLoaded
        +EnsureLoadedAsync() Task
        +LoadReelsCommand
    }

    class MusicSelectionDialogViewModel {
        -IAudioLibraryService _audioLibrary
        +ObservableCollection~MusicTrackModel~ AvailableTracks
        +MusicTrackModel? SelectedTrack
        +LoadTracksCommand
        +SelectTrackCommand
    }

    %% ── Views ──
    class ReelsEditingPage {
        <<Page>>
        -MediaPlayer _musicPreviewPlayer
        -DispatcherTimer _videoProgressTimer
        -MediaPlayer? _subscribedVideoPlayer
        -bool _isSyncingVideoControls
        +ReelsEditingViewModel ViewModel
        +ReelGalleryViewModel GalleryViewModel
        +MusicSelectionDialogViewModel MusicDialogViewModel
        +ReelsEditingPage()
        -Page_Loaded(object, RoutedEventArgs) void
        -Page_Unloaded(object, RoutedEventArgs) void
        -UpdatePanelVisibility() void
        -ReelGridView_SelectionChanged(object, SelectionChangedEventArgs) void
        -ReelGridView_ItemClick(object, ItemClickEventArgs) void
        -LoadVideo(string videoUrl) void
        -StopVideo() void
        +GetStatusVisibility(bool hasStatus) Visibility
        -AttachVideoPlayerEvents() void
        -DetachVideoPlayerEvents() void
        -ReelPlayer_MediaOpened(MediaPlayer, object) void
        -ReelPlayer_MediaEnded(MediaPlayer, object) void
        -VideoProgressTimer_Tick(object?, object) void
        -VideoPlayPauseButton_Click(object, RoutedEventArgs) void
        -VideoSeekSlider_ValueChanged(object, RangeBaseValueChangedEventArgs) void
        -VideoMuteButton_Checked(object, RoutedEventArgs) void
        -VideoMuteButton_Unchecked(object, RoutedEventArgs) void
        -VideoFullscreenButton_Checked(object, RoutedEventArgs) void
        -VideoFullscreenButton_Unchecked(object, RoutedEventArgs) void
        -UpdateVideoTransportUi() void
        -ResetVideoTransportUi() void
        -OnCropModeEntered() void
        -OnCropModeExited() void
        -OnCropSaveStarted() void
        -OnCropVideoUpdated(string) void
        -CropResumePreview_Click(object, RoutedEventArgs) void
        -CropPausePreview_Click(object, RoutedEventArgs) void
        -CropMarginSlider_ValueChanged(object, RangeBaseValueChangedEventArgs) void
        -CropOverlayRoot_SizeChanged(object, SizeChangedEventArgs) void
        -UpdateCropOverlay() void
        -PlayMusicPreview_Click(object, RoutedEventArgs) void
        -StopMusicPreview_Click(object, RoutedEventArgs) void
        -StopMusicPreview() void
        -ChooseMusicButton_Click(object, RoutedEventArgs) void
    }

    %% ── Relationships ──
    AudioLibraryService ..|> IAudioLibraryService : implements
    VideoProcessingService ..|> IVideoProcessingService : implements
    
    ReelsEditingViewModel --|> ObservableObject : inherits
    ReelGalleryViewModel --|> ObservableObject : inherits
    MusicSelectionDialogViewModel --|> ObservableObject : inherits
    
    VideoProcessingService --> IAudioLibraryService : uses
    
    ReelsEditingViewModel --> ReelRepository : uses
    ReelsEditingViewModel --> IVideoProcessingService : uses
    ReelsEditingViewModel --> IAudioLibraryService : uses
    ReelsEditingViewModel --> VideoEditMetadata : manages
    
    ReelGalleryViewModel --> ReelRepository : uses
    MusicSelectionDialogViewModel --> IAudioLibraryService : uses

    ReelsEditingPage --> ReelsEditingViewModel : DataContext
    ReelsEditingPage --> ReelGalleryViewModel : DataContext
    ReelsEditingPage --> MusicSelectionDialogViewModel : DataContext
```
