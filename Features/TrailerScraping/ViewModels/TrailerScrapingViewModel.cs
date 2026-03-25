using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.TrailerScraping.Services;

namespace ubb_se_2026_meio_ai.Features.TrailerScraping.ViewModels
{
    /// <summary>
    /// ViewModel for the Trailer Scraping admin dashboard.
    /// Owner: Andrei
    /// </summary>
    public partial class TrailerScrapingViewModel : ObservableObject
    {
        private readonly VideoIngestionService _ingestionService;
        private readonly IScrapeJobRepository _repository;

        public TrailerScrapingViewModel(
            VideoIngestionService ingestionService,
            IScrapeJobRepository repository)
        {
            _ingestionService = ingestionService;
            _repository = repository;

            QuickPresets = new List<string>
            {
                "action movie 2024 official trailer",
                "horror movie 2024 official trailer",
                "thriller 2025 official trailer",
                "comedy movie 2024 official trailer",
                "sci-fi movie 2025 official trailer",
                "romance movie 2024 trailer",
            };
        }

        // ── Stats bar ────────────────────────────────────────────────────

        [ObservableProperty]
        private int _totalMovies;

        [ObservableProperty]
        private int _totalReels;

        [ObservableProperty]
        private int _totalJobs;

        [ObservableProperty]
        private int _runningJobs;

        [ObservableProperty]
        private int _completedJobs;

        [ObservableProperty]
        private int _failedJobs;

        // ── Search ──────────────────────────────────────────────────────

        [ObservableProperty]
        private string _searchQuery = "new movie trailer 2024";

        [ObservableProperty]
        private int _maxResults = 5;

        [ObservableProperty]
        private bool _isScraping;

        [ObservableProperty]
        private string _statusText = "Idle";

        public List<string> QuickPresets { get; }

        public List<int> MaxResultsOptions { get; } = new() { 5, 10, 15, 25, 50 };

        // ── Logs ────────────────────────────────────────────────────────

        public ObservableCollection<ScrapeJobLogModel> LogEntries { get; } = new();

        // ── Commands ────────────────────────────────────────────────────

        [RelayCommand]
        private void ApplyPreset(string preset)
        {
            SearchQuery = preset;
        }

        [RelayCommand(CanExecute = nameof(CanStartScrape))]
        private async Task StartScrapeAsync()
        {
            IsScraping = true;
            StatusText = "Scraping...";
            StartScrapeCommand.NotifyCanExecuteChanged();

            try
            {
                await _ingestionService.RunScrapeJobAsync(
                    SearchQuery,
                    MaxResults,
                    onLogEntry: async logEntry =>
                    {
                        // Dispatch to UI thread
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
                        {
                            LogEntries.Insert(0, logEntry);
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
                StatusText = "Idle";
                StartScrapeCommand.NotifyCanExecuteChanged();
                await RefreshAsync();
            }
        }

        private bool CanStartScrape() => !IsScraping;

        [RelayCommand]
        private async Task RefreshAsync()
        {
            try
            {
                DashboardStatsModel stats = await _repository.GetDashboardStatsAsync();
                TotalMovies     = stats.TotalMovies;
                TotalReels      = stats.TotalReels;
                TotalJobs       = stats.TotalJobs;
                RunningJobs     = stats.RunningJobs;
                CompletedJobs   = stats.CompletedJobs;
                FailedJobs      = stats.FailedJobs;

                IList<ScrapeJobLogModel> logs = await _repository.GetAllLogsAsync();
                LogEntries.Clear();
                foreach (var log in logs)
                {
                    LogEntries.Add(log);
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
