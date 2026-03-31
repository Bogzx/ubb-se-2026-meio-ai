# Class Diagram — Gabi (Movie Tournament)

```mermaid
classDiagram
    direction TB

    %% ── Models ──
    class MovieCardModel {
        +int MovieId
        +string Title
        +string PosterUrl
        +int ReleaseYear
        +string Genre
        +string Synopsis
    }

    class MatchPair {
        +MovieCardModel MovieA
        +MovieCardModel MovieB
        +int? WinnerId
        +IsCompleted() bool
        +IsBye() bool
    }

    class TournamentState {
        +List~MatchPair~ PendingMatches
        +List~MatchPair~ CompletedMatches
        +List~MovieCardModel~ CurrentRoundWinners
        +int CurrentRound
    }

    TournamentState --> MatchPair : contains multiple
    MatchPair --> MovieCardModel : contains

    %% ── Services & Repositories ──
    class ITournamentLogicService {
        <<interface>>
        +TournamentState CurrentState
        +bool IsTournamentActive
        +StartTournamentAsync(int userId, int poolSize) Task
        +AdvanceWinnerAsync(int userId, int winnerId) Task
        +GetCurrentMatch() MatchPair
        +IsTournamentComplete() bool
        +GetFinalWinner() MovieCardModel
        +ResetTournament() void
    }

    class TournamentLogicService {
        -IMovieTournamentRepository _repository
        -TournamentState _state
        +StartTournamentAsync(int userId, int poolSize) Task
        +AdvanceWinnerAsync(int userId, int winnerId) Task
    }

    class IMovieTournamentRepository {
        <<interface>>
        +GetTournamentPoolSizeAsync(int userId) Task~int~
        +GetTournamentPoolAsync(int userId, int poolSize) Task~List~MovieCardModel~~
        +BoostMovieScoreAsync(int userId, int movieId, double scoreBoost) Task
    }

    class MovieTournamentRepository {
        -ISqlConnectionFactory _connectionFactory
        +GetTournamentPoolSizeAsync(int userId) Task~int~
        +GetTournamentPoolAsync(int userId, int poolSize) Task~List~MovieCardModel~~
        +BoostMovieScoreAsync(int userId, int movieId, double scoreBoost) Task
    }

    TournamentLogicService ..|> ITournamentLogicService
    MovieTournamentRepository ..|> IMovieTournamentRepository
    TournamentLogicService --> TournamentState : manages
    TournamentLogicService --> IMovieTournamentRepository : uses

    %% ── ViewModels ──
    class MovieTournamentViewModel {
        -ITournamentLogicService _tournamentService
        +Page CurrentPage
    }

    class TournamentSetupViewModel {
        -ITournamentLogicService _tournamentService
        -IMovieTournamentRepository _repository
        +int PoolSize
        +int MaxPoolSize
        +string Bg1, Bg2, Bg3, Bg4
        +StartTournamentCommand() Task
    }

    class TournamentMatchViewModel {
        -ITournamentLogicService _tournamentService
        +MovieCardModel MovieOptionA
        +MovieCardModel MovieOptionB
        +string RoundDisplay
        +SelectMovieCommand(int movieId) Task
    }

    class TournamentWinnerViewModel {
        -ITournamentLogicService _tournamentService
        +MovieCardModel WinnerMovie
        +StartAnotherTournamentCommand() void
    }

    %% ── Pages (Views) ──
    class MovieTournamentPage {
        <<View>>
    }
    class TournamentSetupPage {
        <<View>>
    }
    class TournamentMatchPage {
        <<View>>
    }
    class TournamentWinnerPage {
        <<View>>
    }

    %% ── Relationships ──
    MovieTournamentPage --> MovieTournamentViewModel : DataContext
    TournamentSetupPage --> TournamentSetupViewModel : DataContext
    TournamentMatchPage --> TournamentMatchViewModel : DataContext
    TournamentWinnerPage --> TournamentWinnerViewModel : DataContext

    TournamentSetupViewModel --> ITournamentLogicService : uses
    TournamentSetupViewModel --> IMovieTournamentRepository : uses
    TournamentMatchViewModel --> ITournamentLogicService : uses
    TournamentWinnerViewModel --> ITournamentLogicService : uses
    MovieTournamentViewModel --> ITournamentLogicService : uses
```

## Layer Summary

| Layer | Component | Description |
|---|---|---|
| **Models** | `MovieCardModel` | Shared core model for movie information. |
| | `MatchPair` | Represent a head-to-head match between two movies. |
| | `TournamentState` | Snapshot of the active tournament (pending/completed matches). |
| **Services** | `TournamentLogicService` | Core logic for bracket generation and advancing winners. |
| **Repos** | `MovieTournamentRepository` | SQL-based data access for user preferences and pool selection. |
| **ViewModels** | `Tournament...ViewModel` | Logic for Setup, Match-ups, and the Winner screen. |
| **Views** | `Tournament...Page` | WinUI 3 pages providing the user interface. |
