# Merged Class Diagram — All Features

```mermaid
classDiagram
    direction TB

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  SHARED / CORE                                              ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class ObservableObject {
        <<abstract>>
        +event PropertyChanged
        #OnPropertyChanged() void
    }

    class ISqlConnectionFactory {
        <<interface>>
        +CreateConnectionAsync() Task~SqlConnection~
        +CreateMasterConnectionAsync() Task~SqlConnection~
    }

    class ReelModel {
        +int ReelId
        +int MovieId
        +int CreatorUserId
        +string VideoUrl
        +string ThumbnailUrl
        +string Title
        +string Caption
        +double FeatureDurationSeconds
        +string? CropDataJson
        +int? BackgroundMusicId
        +string Source
        +string? Genre
        +DateTime CreatedAt
        +DateTime? LastEditedAt
        +bool IsLiked
        +int LikeCount
    }

    class MovieCardModel {
        +int MovieId
        +string Title
        +string PosterUrl
        +string PrimaryGenre
        +string Genre
        +int ReleaseYear
        +string Synopsis
        +ToString() string
    }

    class UserMoviePreferenceModel {
        +int UserMoviePreferenceId
        +int UserId
        +int MovieId
        +double Score
        +DateTime LastModified
        +int? ChangeFromPreviousValue
    }

    class UserProfileModel {
        +int UserProfileId
        +int UserId
        +int TotalLikes
        +long TotalWatchTimeSec
        +double AvgWatchTimeSec
        +int TotalClipsViewed
        +double LikeToViewRatio
        +DateTime LastUpdated
    }

    class UserReelInteractionModel {
        +long InteractionId
        +int UserId
        +int ReelId
        +bool IsLiked
        +double WatchDurationSec
        +double WatchPercentage
        +DateTime ViewedAt
    }

    class MusicTrackModel {
        <<Core Model>>
    }

    ReelModel --> ObservableObject : inherits

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Reel Upload (Alex)                                ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class ReelUploadRequest {
        <<DTO>>
        +string LocalFilePath
        +string Title
        +string Caption
        +int UploaderUserId
        +int? MovieId
    }

    class IVideoStorageService {
        <<interface>>
        +UploadVideoAsync(ReelUploadRequest request) Task~ReelModel~
        +ValidateVideoAsync(string localFilePath) Task~bool~
    }

    class VideoStorageService {
        -ISqlConnectionFactory _sqlConnectionFactory
        -string _blobStorageDirectory
        +VideoStorageService(ISqlConnectionFactory sqlConnectionFactory)
        +UploadVideoAsync(ReelUploadRequest request) Task~ReelModel~
        +ValidateVideoAsync(string localFilePath) Task~bool~
    }

    class ReelsUploadViewModel {
        -ISqlConnectionFactory _connectionFactory
        -IVideoStorageService _videoStorageService
        -int currentUserID
        +ObservableCollection~MovieCardModel~ SuggestedMovies
        +string PageTitle
        +string StatusMessage
        +string ReelTitle
        +string ReelCaption
        +MovieCardModel? LinkedMovie
        +string LocalVideoFilePath
        +IAsyncRelayCommand SelectVideoFileCommand
        +IAsyncRelayCommand UploadReelCommand
        +IRelayCommand~MovieCardModel~ SelectMovieCommand
        +IAsyncRelayCommand~string~ SearchMovieCommand
        +ReelsUploadViewModel(ISqlConnectionFactory connectionFactory, IVideoStorageService videoStorageService)
        -SelectVideoFileAsync() Task
        -UploadReelAsync() Task
        -SelectMovie(MovieCardModel movie) void
        -SearchMovieAsync(string partialMovieName) Task
    }

    class ReelsUploadPage {
        <<Page>>
        +ReelsUploadViewModel ViewModel
        +ReelsUploadPage()
        -MovieAutoSuggestBox_TextChanged(AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs) void
        -MovieAutoSuggestBox_SuggestionChosen(AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs) void
    }

    VideoStorageService --> IVideoStorageService : implements
    VideoStorageService --> ISqlConnectionFactory : uses
    ReelsUploadViewModel --> ObservableObject : inherits
    ReelsUploadViewModel --> IVideoStorageService : uses
    ReelsUploadViewModel --> ISqlConnectionFactory : uses
    ReelsUploadPage --> ReelsUploadViewModel : DataContext
    IVideoStorageService --> ReelModel : returns
    IVideoStorageService --> ReelUploadRequest : consumes

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Trailer Scraping (Andrei)                         ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class DashboardStatsModel {
        +int TotalMovies
        +int TotalReels
        +int TotalJobs
        +int RunningJobs
        +int CompletedJobs
        +int FailedJobs
    }

    class ScrapeJobModel {
        +int ScrapeJobId
        +string SearchQuery
        +int MaxResults
        +string Status
        +int MoviesFound
        +int ReelsCreated
        +DateTime StartedAt
        +DateTime? CompletedAt
        +string? ErrorMessage
    }

    class ScrapeJobLogModel {
        +long LogId
        +int ScrapeJobId
        +string Level
        +string Message
        +DateTime Timestamp
    }

    class ScrapedVideoResult {
        +string VideoId
        +string Title
        +string ThumbnailUrl
        +string ChannelTitle
        +string Description
        +string VideoUrl
    }

    class IScrapeJobRepository {
        <<interface>>
        +CreateJobAsync(ScrapeJobModel job) Task~int~
        +UpdateJobAsync(ScrapeJobModel job) Task
        +AddLogEntryAsync(ScrapeJobLogModel log) Task
        +GetAllJobsAsync() Task~IList~ScrapeJobModel~~
        +GetLogsForJobAsync(int jobId) Task~IList~ScrapeJobLogModel~~
        +GetAllLogsAsync() Task~IList~ScrapeJobLogModel~~
        +GetDashboardStatsAsync() Task~DashboardStatsModel~
        +SearchMoviesByNameAsync(string partialName) Task~IList~MovieCardModel~~
        +FindMovieByTitleAsync(string title) Task~int?~
        +ReelExistsByVideoUrlAsync(string videoUrl) Task~bool~
        +InsertScrapedReelAsync(ReelModel reel) Task~int~
        +GetAllMoviesAsync() Task~IList~MovieCardModel~~
        +GetAllReelsAsync() Task~IList~ReelModel~~
    }

    class ScrapeJobRepository {
        -ISqlConnectionFactory _connectionFactory
        +ScrapeJobRepository(ISqlConnectionFactory connectionFactory)
        +CreateJobAsync(ScrapeJobModel job) Task~int~
        +UpdateJobAsync(ScrapeJobModel job) Task
        +AddLogEntryAsync(ScrapeJobLogModel log) Task
        +GetAllJobsAsync() Task~IList~ScrapeJobModel~~
        +GetLogsForJobAsync(int jobId) Task~IList~ScrapeJobLogModel~~
        +GetAllLogsAsync() Task~IList~ScrapeJobLogModel~~
        +GetDashboardStatsAsync() Task~DashboardStatsModel~
        +SearchMoviesByNameAsync(string partialName) Task~IList~MovieCardModel~~
        +FindMovieByTitleAsync(string title) Task~int?~
        +ReelExistsByVideoUrlAsync(string videoUrl) Task~bool~
        +InsertScrapedReelAsync(ReelModel reel) Task~int~
        +GetAllMoviesAsync() Task~IList~MovieCardModel~~
        +GetAllReelsAsync() Task~IList~ReelModel~~
    }

    class IVideoIngestionService {
        <<interface>>
        +IngestVideoFromUrlAsync(string trailerUrl, int movieId) Task~string~
    }

    class VideoIngestionService {
        -YouTubeScraperService _scraper
        -IScrapeJobRepository _repository
        -VideoDownloadService _downloader
        +VideoIngestionService(YouTubeScraperService scraper, IScrapeJobRepository repository, VideoDownloadService downloader)
        +IngestVideoFromUrlAsync(string trailerUrl, int movieId) Task~string~
        +RunScrapeJobAsync(MovieCardModel movie, int maxResults, Func~ScrapeJobLogModel, Task~? onLogEntry) Task~ScrapeJobModel~
    }

    class IWebScraperService {
        <<interface>>
        +ScrapeTrailerUrlsAsync(string movieTitle) Task~IList~string~~
    }

    class YouTubeScraperService {
        -string _apiKey
        +YouTubeScraperService(string apiKey)
        +ScrapeTrailerUrlsAsync(string movieTitle) Task~IList~string~~
        +SearchVideosAsync(string query, int maxResults) Task~IList~ScrapedVideoResult~~
    }

    class VideoDownloadService {
        -string _downloadFolder
        -string _ytdlPath
        -string _ffmpegPath
        -bool _isInitialized
        +string? LastError
        +VideoDownloadService(string? downloadFolder)
        +EnsureDependenciesAsync() Task
        +DownloadVideoAsMp4Async(string youtubeUrl, int maxDurationSeconds) Task~string?~
        -FindDownloadedFile(string stdout) string?
        +GetExpectedFilePath(string videoId) string
    }

    class TrailerScrapingViewModel {
        -VideoIngestionService _ingestionService
        -IScrapeJobRepository _repository
        +int TotalMovies
        +int TotalReels
        +int TotalJobs
        +int RunningJobs
        +int CompletedJobs
        +int FailedJobs
        +string SearchText
        +MovieCardModel? SelectedMovie
        +bool NoMovieFound
        +int MaxResults
        +bool IsScraping
        +string StatusText
        +ObservableCollection~MovieCardModel~ SuggestedMovies
        +List~int~ MaxResultsOptions
        +ObservableCollection~ScrapeJobLogModel~ LogEntries
        +ObservableCollection~MovieCardModel~ MovieTableItems
        +ObservableCollection~ReelModel~ ReelTableItems
        +IAsyncRelayCommand~string~ SearchMoviesCommand
        +IRelayCommand StartScrapeCommand
        +IAsyncRelayCommand RefreshCommand
        +TrailerScrapingViewModel(VideoIngestionService ingestionService, IScrapeJobRepository repository)
        -SearchMoviesAsync(string query) Task
        +SelectMovie(MovieCardModel movie) void
        -StartScrapeAsync() Task
        -CanStartScrape() bool
        -RefreshAsync() Task
        +InitializeAsync() Task
    }

    class TrailerScrapingPage {
        <<Page>>
        +TrailerScrapingViewModel ViewModel
        +TrailerScrapingPage()
        -Page_Loaded(object sender, RoutedEventArgs e) void
        -MovieSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) void
        -MovieSearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) void
        -MovieSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) void
    }

    ScrapeJobRepository --> IScrapeJobRepository : implements
    VideoIngestionService --> IVideoIngestionService : implements
    YouTubeScraperService --> IWebScraperService : implements
    TrailerScrapingViewModel --> ObservableObject : inherits
    VideoIngestionService --> YouTubeScraperService : uses
    VideoIngestionService --> IScrapeJobRepository : uses
    VideoIngestionService --> VideoDownloadService : uses
    TrailerScrapingPage --> TrailerScrapingViewModel : DataContext
    TrailerScrapingViewModel --> VideoIngestionService : uses
    TrailerScrapingViewModel --> IScrapeJobRepository : uses

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Reels Editing (Beatrice)                          ║
    %% ╚══════════════════════════════════════════════════════════════╝

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

    class ReelRepository {
        -ISqlConnectionFactory _db
        +ReelRepository(ISqlConnectionFactory db)
        +GetUserReelsAsync(int userId) Task~IList~ReelModel~~
        +UpdateReelEditsAsync(int reelId, string cropDataJson, int? musicId, string? videoUrl) Task~int~
        +GetReelByIdAsync(int reelId) Task~ReelModel?~
        +DeleteReelAsync(int reelId) Task
    }

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

    AudioLibraryService --> IAudioLibraryService : implements
    VideoProcessingService --> IVideoProcessingService : implements
    ReelsEditingViewModel --> ObservableObject : inherits
    ReelGalleryViewModel --> ObservableObject : inherits
    MusicSelectionDialogViewModel --> ObservableObject : inherits
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

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Movie Swipe (Bogdan)                              ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class Bogdan_IPreferenceRepository {
        <<interface>>
        +GetPreferenceAsync(int userId, int movieId) Task~UserMoviePreferenceModel?~
        +UpsertPreferenceAsync(UserMoviePreferenceModel preference) Task
        +GetMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
        +GetAllPreferencesExceptUserAsync(int excludeUserId) Task~Dictionary~int, List~UserMoviePreferenceModel~~~
        +GetUnswipedMovieIdsAsync(int userId) Task~List~int~~
    }

    class Bogdan_PreferenceRepository {
        -ISqlConnectionFactory _connectionFactory
        +PreferenceRepository(ISqlConnectionFactory connectionFactory)
        +GetPreferenceAsync(int userId, int movieId) Task~UserMoviePreferenceModel?~
        +UpsertPreferenceAsync(UserMoviePreferenceModel preference) Task
        +GetMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
        +GetAllPreferencesExceptUserAsync(int excludeUserId) Task~Dictionary~int, List~UserMoviePreferenceModel~~~
        +GetUnswipedMovieIdsAsync(int userId) Task~List~int~~
    }

    class ISwipeService {
        <<interface>>
        +UpdatePreferenceScoreAsync(int userId, int movieId, bool isLiked) Task
        +GetMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
    }

    class SwipeService {
        +double LikeDelta = 1.0$
        +double SkipDelta = -0.5$
        -IPreferenceRepository _preferenceRepository
        +SwipeService(IPreferenceRepository preferenceRepository)
        +UpdatePreferenceScoreAsync(int userId, int movieId, bool isLiked) Task
        +GetMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
    }

    class IMovieCardFeedService {
        <<interface>>
        +FetchMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
    }

    class MovieCardFeedService {
        -IPreferenceRepository _repository
        +MovieCardFeedService(IPreferenceRepository repository)
        +FetchMovieFeedAsync(int userId, int count) Task~List~MovieCardModel~~
    }

    class MovieSwipeViewModel {
        -int BufferSize = 5$
        -int RefillThreshold = 2$
        -int DefaultUserId = 1$
        -ISwipeService _swipeService
        -bool _isRefilling
        +MovieCardModel? CurrentCard
        +ObservableCollection~MovieCardModel~ CardQueue
        +bool IsLoading
        +bool IsAllCaughtUp
        +string StatusMessage
        +IAsyncRelayCommand SwipeRightCommand
        +IAsyncRelayCommand SwipeLeftCommand
        +MovieSwipeViewModel(ISwipeService swipeService)
        -LoadInitialCardsAsync() Task
        -SwipeRightAsync() Task
        -SwipeLeftAsync() Task
        -ProcessSwipeAsync(bool isLiked) Task
        -AdvanceToNextCard() void
        -TryRefillQueueAsync(int? recentlySwipedMovieId) Task
    }

    class MovieSwipeView {
        <<Page>>
        -double SwipeThresholdFraction = 0.30$
        -double FlyOffDistance = 600$
        -int FlyOffDurationMs = 250$
        -bool _isDragging
        -Point _dragStartPoint
        -uint _activePointerId
        +MovieSwipeViewModel ViewModel
        +MovieSwipeView()
        -ViewModel_PropertyChanged(object?, PropertyChangedEventArgs) void
        -UpdateCardContent() void
        -UpdateCardVisibility() void
        -MovieCard_PointerPressed(object, PointerRoutedEventArgs) void
        -MovieCard_PointerMoved(object, PointerRoutedEventArgs) void
        -MovieCard_PointerReleased(object, PointerRoutedEventArgs) void
        -MovieCard_PointerCaptureLost(object, PointerRoutedEventArgs) void
        -FinalizeSwipe() void
        -AnimateCardOffScreen(bool isLiked) void
        -ResetCardPosition() void
    }

    class SwipeResultSummaryView {
        <<Page>>
        +MovieSwipeViewModel ViewModel
        +SwipeResultSummaryView()
    }

    Bogdan_PreferenceRepository --> Bogdan_IPreferenceRepository : implements
    SwipeService --> ISwipeService : implements
    MovieCardFeedService --> IMovieCardFeedService : implements
    MovieSwipeViewModel --> ObservableObject : inherits
    MovieSwipeView --> MovieSwipeViewModel : ViewModel
    SwipeResultSummaryView --> MovieSwipeViewModel : ViewModel
    MovieSwipeViewModel --> ISwipeService : uses
    SwipeService --> Bogdan_IPreferenceRepository : uses
    MovieCardFeedService --> Bogdan_IPreferenceRepository : uses
    Bogdan_PreferenceRepository --> ISqlConnectionFactory : uses

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Movie Tournament (Gabi)                           ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class MatchPair {
        +MovieCardModel MovieA
        +MovieCardModel MovieB
        +int? WinnerId
        +MatchPair(MovieCardModel movieA, MovieCardModel movieB)
        +IsCompleted() bool
        +IsBye() bool
    }

    class TournamentState {
        +List~MatchPair~ PendingMatches
        +List~MatchPair~ CompletedMatches
        +int CurrentRound
        +List~MovieCardModel~ CurrentRoundWinners
        +TournamentState()
    }

    class IMovieTournamentRepository {
        <<interface>>
        +GetTournamentPoolSizeAsync(int userId) Task~int~
        +GetTournamentPoolAsync(int userId, int poolSize) Task~List~MovieCardModel~~
        +BoostMovieScoreAsync(int userId, int movieId, double scoreBoost) Task
    }

    class MovieTournamentRepository {
        -ISqlConnectionFactory _connectionFactory
        +MovieTournamentRepository(ISqlConnectionFactory connectionFactory)
        +GetTournamentPoolSizeAsync(int userId) Task~int~
        +GetTournamentPoolAsync(int userId, int poolSize) Task~List~MovieCardModel~~
        +BoostMovieScoreAsync(int userId, int movieId, double scoreBoost) Task
    }

    class ITournamentLogicService {
        <<interface>>
        +TournamentState CurrentState
        +bool IsTournamentActive
        +StartTournamentAsync(int userId, int poolSize) Task
        +AdvanceWinnerAsync(int userId, int winnerId) Task
        +ResetTournament() void
        +GetCurrentMatch() MatchPair?
        +IsTournamentComplete() bool
        +GetFinalWinner() MovieCardModel
    }

    class TournamentLogicService {
        -IMovieTournamentRepository _repository
        -Random _random
        -TournamentState? _state
        +TournamentLogicService(IMovieTournamentRepository repository)
        +StartTournamentAsync(int userId, int poolSize) Task
        +AdvanceWinnerAsync(int userId, int winnerId) Task
        +ResetTournament() void
        +GetCurrentMatch() MatchPair?
        +IsTournamentComplete() bool
        +GetFinalWinner() MovieCardModel
        -GenerateNextRound() void
    }

    class MovieTournamentViewModel {
        -ITournamentLogicService _tournamentService
        -IMovieTournamentRepository _repository
        -int _currentUserId
        +string PageTitle
        +int CurrentViewState
        +int PoolSize
        +int MaxPoolSize
        +string SetupErrorMessage
        +string? Bg1
        +string? Bg2
        +string? Bg3
        +string? Bg4
        +MovieCardModel? MovieOptionA
        +MovieCardModel? MovieOptionB
        +int RoundNumber
        +string RoundDisplay
        +MovieCardModel? WinnerMovie
        +StartTournamentCommand
        +SelectMovieCommand
        +ResetTournamentCommand
        -LoadMaxPoolSizeAsync() void
        -UpdateCurrentMatchDisplay() void
        +GetImageSource(string? url) ImageSource?
    }

    class TournamentSetupViewModel {
        -ITournamentLogicService _tournamentService
        -IMovieTournamentRepository _repository
        -int _currentUserId
        +int PoolSize
        +int MaxPoolSize
        +string SetupErrorMessage
        +string? Bg1
        +string? Bg2
        +string? Bg3
        +string? Bg4
        +event EventHandler? TournamentStarted
        +StartTournamentCommand
        -LoadDataAsync() void
        +GetImageSource(string? url) ImageSource?
    }

    class TournamentMatchViewModel {
        -ITournamentLogicService _tournamentService
        -int _currentUserId
        +MovieCardModel? MovieOptionA
        +MovieCardModel? MovieOptionB
        +int RoundNumber
        +string RoundDisplay
        +event EventHandler? TournamentComplete
        +event EventHandler? NavigateBack
        +SelectMovieCommand
        +GoBackCommand
        +RefreshCurrentMatch() void
        +GetImageSource(string? url) ImageSource?
    }

    class TournamentWinnerViewModel {
        -ITournamentLogicService _tournamentService
        +MovieCardModel? WinnerMovie
        +event EventHandler? NavigateToSetup
        +StartAnotherTournamentCommand
        +GetImageSource(string? url) ImageSource?
    }

    class MovieTournamentPage {
        <<Page>>
        +MovieTournamentPage()
        -OnLoaded(object, RoutedEventArgs) void
    }

    class TournamentSetupPage {
        <<Page>>
        +TournamentSetupViewModel ViewModel
        +TournamentSetupPage()
    }

    class TournamentMatchPage {
        <<Page>>
        +TournamentMatchViewModel ViewModel
        +TournamentMatchPage()
        -MoviePointerEntered(object, PointerRoutedEventArgs) void
        -MoviePointerExited(object, PointerRoutedEventArgs) void
        -AnimateScale(ScaleTransform, double) void
    }

    class TournamentWinnerPage {
        <<Page>>
        +TournamentWinnerViewModel ViewModel
        +TournamentWinnerPage()
    }

    MovieTournamentRepository --> IMovieTournamentRepository : implements
    TournamentLogicService --> ITournamentLogicService : implements
    MovieTournamentViewModel --> ObservableObject : inherits
    TournamentSetupViewModel --> ObservableObject : inherits
    TournamentMatchViewModel --> ObservableObject : inherits
    TournamentWinnerViewModel --> ObservableObject : inherits
    TournamentLogicService --> IMovieTournamentRepository : uses
    TournamentLogicService --> TournamentState : manages
    TournamentLogicService --> MatchPair : manages
    MovieTournamentViewModel --> ITournamentLogicService : uses
    MovieTournamentViewModel --> IMovieTournamentRepository : uses
    TournamentSetupViewModel --> ITournamentLogicService : uses
    TournamentSetupViewModel --> IMovieTournamentRepository : uses
    TournamentMatchViewModel --> ITournamentLogicService : uses
    TournamentWinnerViewModel --> ITournamentLogicService : uses
    TournamentSetupPage --> TournamentSetupViewModel : DataContext
    TournamentMatchPage --> TournamentMatchViewModel : DataContext
    TournamentWinnerPage --> TournamentWinnerViewModel : DataContext

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Personality Match (Madi)                          ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class MatchResult {
        +int MatchedUserId
        +string MatchedUsername
        +double MatchScore
        +string FacebookAccount
        +bool IsSelfView
    }

    class MoviePreferenceDisplayModel {
        +int MovieId
        +string Title
        +double Score
        +bool IsBestMovie
    }

    class UserAccountModel {
        +int UserId
        +string Username
        +string FacebookAccount
    }

    class IPersonalityMatchRepository {
        <<interface>>
        +GetAllPreferencesExceptUserAsync(int excludeUserId) Task~Dictionary~int, List~UserMoviePreferenceModel~~~
        +GetCurrentUserPreferencesAsync(int userId) Task~List~UserMoviePreferenceModel~~
        +GetUserProfileAsync(int userId) Task~UserProfileModel?~
        +GetRandomUserIdsAsync(int excludeUserId, int count) Task~List~int~~
        +GetUsernameAsync(int userId) Task~string~
        +GetTopPreferencesWithTitlesAsync(int userId, int count) Task~List~MoviePreferenceDisplayModel~~
    }

    class PersonalityMatchRepository {
        -ISqlConnectionFactory _connectionFactory
        +PersonalityMatchRepository(ISqlConnectionFactory connectionFactory)
        +GetAllPreferencesExceptUserAsync(int excludeUserId) Task~Dictionary~int, List~UserMoviePreferenceModel~~~
        +GetCurrentUserPreferencesAsync(int userId) Task~List~UserMoviePreferenceModel~~
        +GetUserProfileAsync(int userId) Task~UserProfileModel?~
        +GetRandomUserIdsAsync(int excludeUserId, int count) Task~List~int~~
        +GetUsernameAsync(int userId) Task~string~
        +GetTopPreferencesWithTitlesAsync(int userId, int count) Task~List~MoviePreferenceDisplayModel~~
    }

    class IPersonalityMatchingService {
        <<interface>>
        +GetTopMatchesAsync(int userId, int count) Task~List~MatchResult~~
        +GetRandomUsersAsync(int userId, int count) Task~List~MatchResult~~
    }

    class PersonalityMatchingService {
        -IPersonalityMatchRepository _repository
        -Dictionary~int, string~ HardcodedUsernames
        -Dictionary~int, string~ HardcodedFacebookAccounts
        +PersonalityMatchingService(IPersonalityMatchRepository repository)
        +GetTopMatchesAsync(int userId, int count) Task~List~MatchResult~~
        +GetRandomUsersAsync(int userId, int count) Task~List~MatchResult~~
        -ComputeCosineSimilarity(Dictionary~int, double~ vectorA, Dictionary~int, double~ vectorB) double
        -GetHardcodedUsername(int userId) string
        -GetFacebookAccount(int userId) string
    }

    class PersonalityMatchViewModel {
        -IPersonalityMatchingService _matchingService
        -Dictionary~int, (string Username, string FacebookAccount)~ _demoAccounts
        -int _activeUserId
        -List~int~ _loggedAccountIds
        +string PageTitle
        +string StatusMessage
        +bool IsLoading
        +bool ShowNoMatch
        +bool HasMatches
        +bool IsAccountPanelOpen
        +string CurrentUsername
        +string CurrentFacebookAccount
        +ObservableCollection~MatchResult~ MatchResults
        +ObservableCollection~MatchResult~ FallbackUsers
        +ObservableCollection~UserAccountModel~ OtherAccounts
        +event Action~MatchResult~? NavigateToDetail
        +event Action~UserAccountModel~? NavigateToCurrentUserDetail
        +LoadMatchesCommand
        +ViewUserDetailCommand
        +ToggleAccountPanelCommand
        +SwitchAccountCommand
        +ViewCurrentAccountDetailCommand
        +AddAccount(UserAccountModel account) void
        +GetAvailableAccountsToAdd() IReadOnlyList~UserAccountModel~
        -RefreshAccountCollections() void
    }

    class MatchedUserDetailViewModel {
        -IPersonalityMatchRepository _repository
        +string MatchedUsername
        +double MatchScore
        +string FacebookAccount
        +UserProfileModel? UserProfile
        +bool IsLoading
        +string? ErrorMessage
        +bool HasProfile
        +bool ShowCompatibility
        +ObservableCollection~MoviePreferenceDisplayModel~ TopPreferences
        +MatchedUserDetailViewModel(IPersonalityMatchRepository repository)
        +LoadUserDetailAsync(int userId, double matchScore, string facebookAccount, string username, bool isSelfView) Task
    }

    class PersonalityMatchPage {
        <<Page>>
        +PersonalityMatchViewModel ViewModel
        +PersonalityMatchPage()
        -Page_Loaded(object sender, RoutedEventArgs e) void
        -MatchListView_ItemClick(object sender, ItemClickEventArgs e) void
        -FallbackListView_ItemClick(object sender, ItemClickEventArgs e) void
        -OtherAccountsList_ItemClick(object sender, ItemClickEventArgs e) void
        -AddAccount_Click(object sender, RoutedEventArgs e) void
        -OnNavigateToDetail(MatchResult match) void
        -OnNavigateToCurrentUserDetail(UserAccountModel account) void
    }

    class MatchedUserDetailPage {
        <<Page>>
        +MatchedUserDetailViewModel ViewModel
        +MatchedUserDetailPage()
        #OnNavigatedTo(NavigationEventArgs e) void
        -BackButton_Click(object sender, RoutedEventArgs e) void
    }

    PersonalityMatchRepository --> IPersonalityMatchRepository : implements
    PersonalityMatchingService --> IPersonalityMatchingService : implements
    PersonalityMatchingService --> IPersonalityMatchRepository : uses
    PersonalityMatchingService --> MatchResult : manages
    PersonalityMatchViewModel --> ObservableObject : inherits
    MatchedUserDetailViewModel --> ObservableObject : inherits
    PersonalityMatchViewModel --> IPersonalityMatchingService : uses
    PersonalityMatchViewModel --> MatchResult : manages
    PersonalityMatchViewModel --> UserAccountModel : manages
    MatchedUserDetailViewModel --> IPersonalityMatchRepository : uses
    MatchedUserDetailViewModel --> MoviePreferenceDisplayModel : manages
    MatchedUserDetailViewModel --> UserProfileModel : manages
    PersonalityMatchPage --> PersonalityMatchViewModel : DataContext
    MatchedUserDetailPage --> MatchedUserDetailViewModel : DataContext

    %% ╔══════════════════════════════════════════════════════════════╗
    %% ║  FEATURE: Reels Feed (Tudor)                                ║
    %% ╚══════════════════════════════════════════════════════════════╝

    class IInteractionRepository {
        <<interface>>
        +InsertInteractionAsync(UserReelInteractionModel interaction) Task
        +UpsertInteractionAsync(int userId, int reelId) Task
        +ToggleLikeAsync(int userId, int reelId) Task
        +UpdateViewDataAsync(int userId, int reelId, double watchDurationSec, double watchPercentage) Task
        +GetInteractionAsync(int userId, int reelId) Task~UserReelInteractionModel?~
        +GetLikeCountAsync(int reelId) Task~int~
        +GetReelMovieIdAsync(int reelId) Task~int?~
    }

    class InteractionRepository {
        -ISqlConnectionFactory _connectionFactory
    }

    class Tudor_IPreferenceRepository {
        <<interface>>
        +BoostPreferenceOnLikeAsync(int userId, int movieId) Task
    }

    class Tudor_PreferenceRepository {
        -ISqlConnectionFactory _connectionFactory
        -double LikeBoostAmount
    }

    class IProfileRepository {
        <<interface>>
        +GetProfileAsync(int userId) Task~UserProfileModel?~
        +UpsertProfileAsync(UserProfileModel profile) Task
    }

    class ProfileRepository {
        -ISqlConnectionFactory _connectionFactory
    }

    class IClipPlaybackService {
        <<interface>>
        +bool IsPlaying
        +PlayAsync(string videoUrl) Task
        +PauseAsync() Task
        +ResumeAsync() Task
        +SeekAsync(double positionSeconds) Task
        +GetElapsedSeconds() double
        +PrefetchClipAsync(string videoUrl) Task
    }

    class ClipPlaybackService {
        -Dictionary~string, MediaSource~ _prefetchedSources
        -Stopwatch _elapsed
        +GetMediaSource(string videoUrl) MediaSource
        +Dispose() void
    }

    class IEngagementProfileService {
        <<interface>>
        +GetProfileAsync(int userId) Task~UserProfileModel?~
        +RefreshProfileAsync(int userId) Task
    }

    class EngagementProfileService {
        -IProfileRepository _profileRepository
        -ISqlConnectionFactory _connectionFactory
        -AggregateInteractionStatsAsync(int userId) Task~UserProfileModel~
    }

    class IRecommendationService {
        <<interface>>
        +GetRecommendedReelsAsync(int userId, int count) Task~IList~ReelModel~~
    }

    class RecommendationService {
        -ISqlConnectionFactory _connectionFactory
        -UserHasPreferencesAsync(int userId) Task~bool~
        -GetPersonalizedReelsAsync(int userId, int count) Task~IList~ReelModel~~
        -GetColdStartReelsAsync(int userId, int count) Task~IList~ReelModel~~
        -ExecuteReelQueryAsync(string sql, Action~SqlCommand~ configureParameters) Task~IList~ReelModel~~
    }

    class IReelInteractionService {
        <<interface>>
        +ToggleLikeAsync(int userId, int reelId) Task
        +RecordViewAsync(int userId, int reelId, double watchDurationSec, double watchPercentage) Task
        +GetInteractionAsync(int userId, int reelId) Task~UserReelInteractionModel?~
        +GetLikeCountAsync(int reelId) Task~int~
    }

    class ReelInteractionService {
        -IInteractionRepository _interactionRepository
        -IPreferenceRepository _preferenceRepository
    }

    class ReelsFeedViewModel {
        -int MockUserId = 1$
        -IRecommendationService _recommendationService
        -IClipPlaybackService _clipPlaybackService
        -IReelInteractionService _reelInteractionService
        -Stopwatch _watchStopwatch
        -ReelModel? _previousReel
        +string PageTitle
        +string StatusMessage
        +bool IsLoading
        +string? ErrorMessage
        +bool HasError
        +bool IsEmpty
        +ReelModel? CurrentReel
        +ObservableCollection~ReelModel~ ReelQueue
        +LoadFeedCommand
        +ScrollNextCommand
        +ScrollPreviousCommand
        -PrefetchNearby(int currentIndex) void
        -FlushWatchData() void
        +OnNavigatingAway() void
        -LoadLikeDataAsync(IList~ReelModel~ reels) Task
    }

    class UserProfileViewModel {
        -IEngagementProfileService _profileService
        +UserProfileModel? Profile
        +bool IsLoading
        +string? ErrorMessage
        +LoadProfileCommand
    }

    class ReelsFeedPage {
        <<Page>>
        +ReelsFeedViewModel ViewModel
        +ReelsFeedPage()
        -ReelsFeedPage_Loaded(object sender, RoutedEventArgs e) void
        -ReelsFeedPage_Unloaded(object sender, RoutedEventArgs e) void
        -RetryButton_Click(object sender, RoutedEventArgs e) void
        -FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e) void
        -TriggerPlaybackForCurrent() void
        -FindVisualChild~T~(DependencyObject parent) T?
    }

    class ReelItemView {
        <<UserControl>>
        +ReelModel Reel
        +static bool IsAppClosing
        -IClipPlaybackService _playbackService
        -IReelInteractionService _interactionService
        -DispatcherTimer? _progressTimer
        -volatile bool _disposed
        -ReelModel? _subscribedReel
        +ReelItemView()
        -OnReelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) void$
        -OnReelPropertyChanged(object sender, PropertyChangedEventArgs args) void
        -UnsubscribeFromReel() void
        +PlayVideo() void
        +PauseVideo() void
        +DisposeMediaPlayer() void
        -UpdateGenreBadge(string? genre) void
        -UpdateLikeVisuals(bool isLiked, int likeCount) void
        -LikeButton_Click(object sender, RoutedEventArgs e) void
        -ReelItemView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e) void
        -ToggleLikeWithAnimationAsync() Task
        -PlayHeartBounceAnimation() void
        -PlayHeartBurstAnimation() void
        -StartProgressTimer() void
        -StopProgressTimer() void
        -ProgressTimer_Tick(object sender, object e) void
        -DisposeCurrentPlayer() void
        -GetParentFlipView(DependencyObject element) FlipView?
        -MediaPlayer_MediaEnded(MediaPlayer sender, object args) void
    }

    InteractionRepository --> IInteractionRepository : implements
    Tudor_PreferenceRepository --> Tudor_IPreferenceRepository : implements
    ProfileRepository --> IProfileRepository : implements
    ClipPlaybackService --> IClipPlaybackService : implements
    ClipPlaybackService --> IDisposable : implements
    EngagementProfileService --> IEngagementProfileService : implements
    EngagementProfileService --> IProfileRepository : uses
    RecommendationService --> IRecommendationService : implements
    ReelInteractionService --> IReelInteractionService : implements
    ReelInteractionService --> IInteractionRepository : uses
    ReelInteractionService --> Tudor_IPreferenceRepository : uses
    ReelsFeedViewModel --> ObservableObject : inherits
    UserProfileViewModel --> ObservableObject : inherits
    ReelsFeedViewModel --> IRecommendationService : uses
    ReelsFeedViewModel --> IClipPlaybackService : uses
    ReelsFeedViewModel --> IReelInteractionService : uses
    UserProfileViewModel --> IEngagementProfileService : uses
    ReelsFeedPage --> ReelsFeedViewModel : DataContext
    ReelItemView --> IClipPlaybackService : uses
    ReelItemView --> IReelInteractionService : uses
```
