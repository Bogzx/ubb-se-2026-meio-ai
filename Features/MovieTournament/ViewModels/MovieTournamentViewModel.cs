using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Services;

namespace ubb_se_2026_meio_ai.Features.MovieTournament.ViewModels
{
    /// <summary>
    /// Unified view model for the movie tournament feature.
    /// Manages three view states: Setup (0), Match (1), and Winner (2),
    /// driven by a single <see cref="CurrentViewState"/> property.
    /// </summary>
    public partial class MovieTournamentViewModel : ObservableObject
    {
        private const int SetupViewState = 0;
        private const int MatchViewState = 1;
        private const int WinnerViewState = 2;
        private const int MinimumPoolSize = 4;
        private const int BackgroundImageCount = 4;
        private const int CurrentUserId = 1;

        private const string FallbackPoster1 = "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg";
        private const string FallbackPoster2 = "https://media.themoviedb.org/t/p/w600_and_h900_face/qJ2tW6WMUDux911r6m7haRef0WH.jpg";
        private const string FallbackPoster3 = "https://media.themoviedb.org/t/p/w600_and_h900_face/q2qXg4OmJgm0qGaBYLdXzP8nHPy.jpg";
        private const string FallbackPoster4 = "https://media.themoviedb.org/t/p/w600_and_h900_face/nrmXQ0zcZUL8jFLrakWc90IR8z9.jpg";

        private readonly ITournamentLogicService tournamentLogicService;
        private readonly IMovieTournamentRepository tournamentRepository;

        [ObservableProperty]
        private int currentViewState = SetupViewState;

        [ObservableProperty]
        private int poolSize = MinimumPoolSize;

        [ObservableProperty]
        private int maxPoolSize = MinimumPoolSize;

        [ObservableProperty]
        private string setupErrorMessage = string.Empty;

        [ObservableProperty]
        private string? backgroundPoster1;

        [ObservableProperty]
        private string? backgroundPoster2;

        [ObservableProperty]
        private string? backgroundPoster3;

        [ObservableProperty]
        private string? backgroundPoster4;

        [ObservableProperty]
        private MovieCardModel? movieOptionA;

        [ObservableProperty]
        private MovieCardModel? movieOptionB;

        [ObservableProperty]
        private string roundDisplay = string.Empty;

        [ObservableProperty]
        private MovieCardModel? winnerMovie;

        public MovieTournamentViewModel(
            ITournamentLogicService tournamentLogicService,
            IMovieTournamentRepository tournamentRepository)
        {
            this.tournamentLogicService = tournamentLogicService;
            this.tournamentRepository = tournamentRepository;

            if (this.tournamentLogicService.IsTournamentActive)
            {
                this.UpdateCurrentMatchDisplay();
                this.CurrentViewState = MatchViewState;
            }
            else if (this.tournamentLogicService.IsTournamentComplete())
            {
                this.WinnerMovie = this.tournamentLogicService.GetFinalWinner();
                this.CurrentViewState = WinnerViewState;
            }
            else
            {
                _ = this.LoadSetupDataAsync();
            }
        }

        public ImageSource? GetImageSource(string? posterUrl)
        {
            if (string.IsNullOrWhiteSpace(posterUrl))
            {
                return null;
            }

            try
            {
                return new BitmapImage(new Uri(posterUrl));
            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        [RelayCommand]
        public async Task StartTournamentAsync()
        {
            if (this.PoolSize < MinimumPoolSize)
            {
                this.SetupErrorMessage = $"Pool size must be at least {MinimumPoolSize}.\nIf you don't have enough, go like some movies!";
                return;
            }

            if (this.PoolSize > this.MaxPoolSize)
            {
                this.SetupErrorMessage = $"Pool size cannot exceed {this.MaxPoolSize}.";
                return;
            }

            this.SetupErrorMessage = string.Empty;

            try
            {
                await this.tournamentLogicService.StartTournamentAsync(CurrentUserId, this.PoolSize);
                this.UpdateCurrentMatchDisplay();
                this.CurrentViewState = MatchViewState;
            }
            catch (Exception exception)
            {
                this.SetupErrorMessage = $"Failed to start tournament: {exception.Message}";
            }
        }

        [RelayCommand]
        public async Task SelectMovieAsync(int movieId)
        {
            await this.tournamentLogicService.AdvanceWinnerAsync(CurrentUserId, movieId);

            if (this.tournamentLogicService.IsTournamentComplete())
            {
                this.WinnerMovie = this.tournamentLogicService.GetFinalWinner();
                this.CurrentViewState = WinnerViewState;
            }
            else
            {
                this.UpdateCurrentMatchDisplay();
            }
        }

        [RelayCommand]
        public void ResetTournament()
        {
            this.tournamentLogicService.ResetTournament();
            this.CurrentViewState = SetupViewState;
            _ = this.LoadSetupDataAsync();
        }

        public async Task LoadSetupDataAsync()
        {
            try
            {
                await Task.Yield();

                this.MaxPoolSize = await this.tournamentRepository.GetTournamentPoolSizeAsync(CurrentUserId);

                var backgroundMovies = await this.tournamentRepository.GetTournamentPoolAsync(
                    CurrentUserId, BackgroundImageCount);

                var posters = new List<string?>
                {
                    FallbackPoster1,
                    FallbackPoster2,
                    FallbackPoster3,
                    FallbackPoster4
                };

                for (int i = 0; i < backgroundMovies.Count && i < BackgroundImageCount; i++)
                {
                    if (!string.IsNullOrWhiteSpace(backgroundMovies[i].PosterUrl))
                    {
                        posters[i] = backgroundMovies[i].PosterUrl;
                    }
                }

                this.BackgroundPoster1 = posters[0];
                this.BackgroundPoster2 = posters[1];
                this.BackgroundPoster3 = posters[2];
                this.BackgroundPoster4 = posters[3];
            }
            catch (Exception exception)
            {
                this.SetupErrorMessage = $"Error loading pool data: {exception.Message}";
            }
        }

        private void UpdateCurrentMatchDisplay()
        {
            var currentMatch = this.tournamentLogicService.GetCurrentMatch();
            if (currentMatch == null)
            {
                return;
            }

            this.MovieOptionA = currentMatch.FirstMovie;
            this.MovieOptionB = currentMatch.SecondMovie;
            this.RoundDisplay = $"Round {this.tournamentLogicService.CurrentState.CurrentRound}";
        }
    }
}