using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ubb_se_2026_meio_ai.Features.MovieTournament.Services;

namespace ubb_se_2026_meio_ai.Features.MovieTournament.ViewModels
{
    /// <summary>
    /// ViewModel for the Movie Tournament page.
    /// Owner: Gabi
    /// </summary>
    public partial class MovieTournamentViewModel : ObservableObject
    {
        private readonly ITournamentLogicService _tournamentService;
        private readonly IMovieTournamentRepository _repository;
        private readonly int _currentUserId = 1; // Hardcoded for now until authentication is added

        [ObservableProperty]
        private string _pageTitle = "Movie Tournament";

        // View States: 0 = Setup, 1 = Match, 2 = Winner
        [ObservableProperty]
        private int _currentViewState = 0;

        // --- Setup State ---
        [ObservableProperty]
        private int _poolSize = 8;

        [ObservableProperty]
        private int _maxPoolSize = 0;

        [ObservableProperty]
        private string _setupErrorMessage = string.Empty;

        [ObservableProperty] private string? _bg1;
        [ObservableProperty] private string? _bg2;
        [ObservableProperty] private string? _bg3;
        [ObservableProperty] private string? _bg4;

        // --- Match State ---
        [ObservableProperty]
        private Models.MovieCard? _movieOptionA;

        [ObservableProperty]
        private Models.MovieCard? _movieOptionB;

        [ObservableProperty]
        private int _roundNumber;

        [ObservableProperty]
        private string _roundDisplay = string.Empty;

        // --- Winner State ---
        [ObservableProperty]
        private Models.MovieCard? _winnerMovie;

        public MovieTournamentViewModel(
            ITournamentLogicService tournamentService,
            IMovieTournamentRepository repository)
        {
            _tournamentService = tournamentService;
            _repository = repository;

            if (_tournamentService.IsTournamentActive)
            {
                UpdateCurrentMatchDisplay();
                CurrentViewState = 1; // Go right to active match summary
            }
            else if (_tournamentService.IsTournamentComplete())
            {
                WinnerMovie = _tournamentService.GetFinalWinner();
                CurrentViewState = 2; // Show the winner
            }
            else
            {
                // Load initial data
                LoadMaxPoolSizeAsync();
            }
        }

        private async void LoadMaxPoolSizeAsync()
        {
            try
            {
                MaxPoolSize = await _repository.GetTournamentPoolSizeAsync(_currentUserId);
                
                // Load 4 posters for background
                var bgMovies = await _repository.GetTournamentPoolAsync(_currentUserId, 4);
                if (bgMovies.Count >= 4)
                {
                    Bg1 = bgMovies[0].PosterUrl;
                    Bg2 = bgMovies[1].PosterUrl;
                    Bg3 = bgMovies[2].PosterUrl;
                    Bg4 = bgMovies[3].PosterUrl;
                }
            }
            catch (Exception ex)
            {
                SetupErrorMessage = "Error loading pool size: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task StartTournamentAsync()
        {
            if (PoolSize < 5)
            {
                SetupErrorMessage = "Pool size must be at least 5.";
                return;
            }
            
            if (PoolSize > MaxPoolSize)
            {
                SetupErrorMessage = $"Pool size cannot exceed {MaxPoolSize}.";
                return;
            }

            SetupErrorMessage = string.Empty;

            try
            {
                await _tournamentService.StartTournamentAsync(_currentUserId, PoolSize);
                UpdateCurrentMatchDisplay();
                CurrentViewState = 1; // Go to Match UI
            }
            catch (Exception ex)
            {
                SetupErrorMessage = "Failed to start tournament: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task SelectMovieAsync(int movieId)
        {
            await _tournamentService.AdvanceWinnerAsync(_currentUserId, movieId);

            if (_tournamentService.IsTournamentComplete())
            {
                WinnerMovie = _tournamentService.GetFinalWinner();
                CurrentViewState = 2; // Go to Winner UI
            }
            else
            {
                UpdateCurrentMatchDisplay();
            }
        }

        [RelayCommand]
        private void ResetTournament()
        {
            _tournamentService.ResetTournament();
            CurrentViewState = 0; // Go back to Setup UI
            LoadMaxPoolSizeAsync(); // Refresh just in case
        }

        private void UpdateCurrentMatchDisplay()
        {
            var match = _tournamentService.GetCurrentMatch();
            if (match != null)
            {
                MovieOptionA = match.MovieA;
                MovieOptionB = match.MovieB;
                RoundNumber = _tournamentService.CurrentState.CurrentRound;
                RoundDisplay = $"Round {RoundNumber}";
            }
        }

        public ImageSource? GetImageSource(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            try
            {
                return new BitmapImage(new Uri(url));
            }
            catch
            {
                return null;
            }
        }
    }
}
