# Class Diagram — Alex (Reel Upload)

```mermaid
classDiagram
    direction TB

    %% ── Models ──
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

    class ReelUploadRequest {
        <<DTO>>
        +string LocalFilePath
        +string Title
        +string Caption
        +int UploaderUserId
        +int? MovieId
    }

    %% ── Services ──
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

    %% ── ViewModel ──
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

    %% ── View ──
    class ReelsUploadPage {
        <<Page>>
        +ReelsUploadViewModel ViewModel
        +ReelsUploadPage()
        -MovieAutoSuggestBox_TextChanged(AutoSuggestBox, AutoSuggestBoxTextChangedEventArgs) void
        -MovieAutoSuggestBox_SuggestionChosen(AutoSuggestBox, AutoSuggestBoxSuggestionChosenEventArgs) void
    }

    %% ── Relationships ──
    ReelModel --|> ObservableObject : inherits
    ReelsUploadViewModel --|> ObservableObject : inherits
    ReelsUploadPage --> ReelsUploadViewModel : DataContext
    ReelsUploadViewModel --> IVideoStorageService : _videoStorageService
    ReelsUploadViewModel --> ISqlConnectionFactory : _connectionFactory
    VideoStorageService ..|> IVideoStorageService : implements
    VideoStorageService --> ISqlConnectionFactory : _sqlConnectionFactory
    IVideoStorageService --> ReelModel : returns
    IVideoStorageService --> ReelUploadRequest : consumes
```
