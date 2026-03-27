# Class Diagram — Gabi (Movie Tournament)

```mermaid
classDiagram
    direction TB

    %% ── Models ──
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

    class MovieCardModel {
        <<Core Model>>
    }

    %% ── Repositories ──
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

    %% ── Services ──
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

    %% ── ViewModels ──
    class ObservableObject {
        <<abstract>>
        +event PropertyChanged
        #OnPropertyChanged() void
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

    %% ── Views ──
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

    %% ── Relationships ──
    MovieTournamentRepository ..|> IMovieTournamentRepository : implements
    TournamentLogicService ..|> ITournamentLogicService : implements
    
    MovieTournamentViewModel --|> ObservableObject : inherits
    TournamentSetupViewModel --|> ObservableObject : inherits
    TournamentMatchViewModel --|> ObservableObject : inherits
    TournamentWinnerViewModel --|> ObservableObject : inherits
    
    TournamentLogicService --> IMovieTournamentRepository : uses
    TournamentLogicService --> TournamentState : manages
    TournamentLogicService --> MatchPair : manages
    TournamentLogicService --> MovieCardModel : manages

    MovieTournamentViewModel --> ITournamentLogicService : uses
    MovieTournamentViewModel --> IMovieTournamentRepository : uses

    TournamentSetupViewModel --> ITournamentLogicService : uses
    TournamentSetupViewModel --> IMovieTournamentRepository : uses

    TournamentMatchViewModel --> ITournamentLogicService : uses

    TournamentWinnerViewModel --> ITournamentLogicService : uses

    TournamentSetupPage --> TournamentSetupViewModel : DataContext
    TournamentMatchPage --> TournamentMatchViewModel : DataContext
    TournamentWinnerPage --> TournamentWinnerViewModel : DataContext
```