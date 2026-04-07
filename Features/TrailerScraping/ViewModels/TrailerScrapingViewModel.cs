using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.Services;

namespace Ubb_se_2026_meio_ai.Features.TrailerScraping.ViewModels
{
    /// <summary>
    /// ViewModel for the Trailer Scraping admin dashboard.
    /// Handles movie autocomplete, scrape execution, and log display.
    /// Owner: Andrei
    /// </summary>
    public partial class TrailerScrapingViewModel : ObservableObject
    {
        private const int DefaultMaxResults = 5;
        private const int MinimumSearchQueryLength = 2;
        private const int EmptyCollectionCount = 0;
        private const int TopLogEntryIndex = 0;

        private const string StatusIdle = "Idle";
        private const string StatusScraping = "Scraping...";

        private readonly VideoIngestionService ingestionService;
        private readonly IScrapeJobRepository repository;

        public TrailerScrapingViewModel(
            VideoIngestionService ingestionService,
            IScrapeJobRepository repository)
        {
            this.ingestionService = ingestionService;
            this.repository = repository;
        }

        // ── Stats bar ────────────────────────────────────────────────────
        [ObservableProperty]
        private int totalMovies;

        [ObservableProperty]
        private int totalReels;

        [ObservableProperty]
        private int totalJobs;

        [ObservableProperty]
        private int runningJobs;

        [ObservableProperty]
        private int completedJobs;

        [ObservableProperty]
        private int failedJobs;

        // ── Movie Autocomplete ──────────────────────────────────────────
        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private MovieCardModel? selectedMovie;

        [ObservableProperty]
        private bool noMovieFound;

        [ObservableProperty]
        private int maxResults = DefaultMaxResults;

        [ObservableProperty]
        private bool isScraping;

        [ObservableProperty]
        private string statusText = StatusIdle;

        public ObservableCollection<MovieCardModel> SuggestedMovies { get; } = new ();

        public List<int> MaxResultsOptions { get; } = new () { 5, 10, 15, 25, 50 };

        // ── Logs ────────────────────────────────────────────────────────
        public ObservableCollection<ScrapeJobLogModel> LogEntries { get; } = new ();

        // ── Commands ────────────────────────────────────────────────────

        /// <summary>
        /// Called when the user types in the AutoSuggestBox.
        /// Queries the Movie table for case-insensitive matches.
        /// </summary>
        [RelayCommand]
        private async Task SearchMoviesAsync(string query)
        {
            SearchText = query;
            SelectedMovie = null;
            NoMovieFound = false;

            if (string.IsNullOrWhiteSpace(query) || query.Length < MinimumSearchQueryLength)
            {
                SuggestedMovies.Clear();
                return;
            }

            try
            {
                IList<MovieCardModel> matches = await repository.SearchMoviesByNameAsync(query);
                SuggestedMovies.Clear();

                foreach (var movieMatch in matches)
                {
                    SuggestedMovies.Add(movieMatch);
                }

                NoMovieFound = SuggestedMovies.Count == EmptyCollectionCount;
            }
            catch
            {
                SuggestedMovies.Clear();
            }
        }

        /// <summary>
        /// Called when the user picks a movie from the dropdown.
        /// </summary>
        public void SelectMovie(MovieCardModel movie)
        {
            SelectedMovie = movie;
            SearchText = movie.Title;
            NoMovieFound = false;
            StartScrapeCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanStartScrape))]
        private async Task StartScrapeAsync()
        {
            if (SelectedMovie is null)
            {
                return;
            }

            IsScraping = true;
            StatusText = StatusScraping;
            StartScrapeCommand.NotifyCanExecuteChanged();

            try
            {
                await ingestionService.RunScrapeJobAsync(
                    SelectedMovie,
                    MaxResults,
                    onLogEntry: async logEntry =>
                    {
                        // Dispatch to UI thread
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                        {
                            LogEntries.Insert(TopLogEntryIndex, logEntry);
                        });
                        await Task.CompletedTask;
                    });
            }
            catch
            {
                // Errors are already logged inside RunScrapeJobAsync
            }
            finally
            {
                IsScraping = false;
                StatusText = StatusIdle;
                StartScrapeCommand.NotifyCanExecuteChanged();
                await RefreshAsync();
            }
        }

        // ── Table viewers ────────────────────────────────────────────
        public ObservableCollection<MovieCardModel> MovieTableItems { get; } = new ();
        public ObservableCollection<ReelModel> ReelTableItems { get; } = new ();

        private bool CanStartScrape() => !IsScraping && SelectedMovie is not null;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                DashboardStatsModel stats = await repository.GetDashboardStatsAsync();
                TotalMovies = stats.TotalMovies;
                TotalReels = stats.TotalReels;
                TotalJobs = stats.TotalJobs;
                RunningJobs = stats.RunningJobs;
                CompletedJobs = stats.CompletedJobs;
                FailedJobs = stats.FailedJobs;

                IList<ScrapeJobLogModel> logs = await repository.GetAllLogsAsync();
                LogEntries.Clear();
                foreach (var log in logs)
                {
                    LogEntries.Add(log);
                }

                // Table viewers
                IList<MovieCardModel> movies = await repository.GetAllMoviesAsync();
                MovieTableItems.Clear();

                foreach (var movie in movies)
                {
                    MovieTableItems.Add(movie);
                }

                IList<ReelModel> reels = await repository.GetAllReelsAsync();
                ReelTableItems.Clear();

                foreach (var reel in reels)
                {
                    ReelTableItems.Add(reel);
                }
            }
            catch
            {
                // Database may not be available during development
            }
        }

        /// <summary>
        /// Called by the Page when it loads to populate initial data.
        /// </summary>
        public async Task InitializeAsync()
        {
            await RefreshAsync();
        }
    }
}