using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.MovieSwipe.Services;

namespace ubb_se_2026_meio_ai.Features.MovieSwipe.ViewModels
{
    public partial class MovieSwipeViewModel : ObservableObject
    {
        private const int BufferSize = 5;
        private const int RefillThreshold = 2;
        private const int DefaultUserId = 1;

        private readonly ISwipeService _swipeService;
        private bool _isRefilling;

        public MovieSwipeViewModel(ISwipeService swipeService)
        {
            _swipeService = swipeService;
            CardQueue = new ObservableCollection<MovieCardModel>();

            _ = LoadInitialCardsAsync();
        }

        [ObservableProperty]
        private MovieCardModel? _currentCard;

        public ObservableCollection<MovieCardModel> CardQueue { get; }

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isAllCaughtUp;

        [ObservableProperty]
        private string _statusMessage = "Swipe right to like, left to skip.";

        private async Task LoadInitialCardsAsync()
        {
            try
            {
                IsLoading = true;
                IsAllCaughtUp = false;

                var movies = await _swipeService.GetMovieFeedAsync(DefaultUserId, BufferSize);

                CardQueue.Clear();
                foreach (var movie in movies)
                {
                    CardQueue.Add(movie);
                }

                AdvanceToNextCard();

                if (CardQueue.Count == 0 && CurrentCard == null)
                {
                    IsAllCaughtUp = true;
                    StatusMessage = "No movies found in database.";
                }
            }
            catch (Exception)
            {
                StatusMessage = "Could not load movies. Please try again later.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SwipeRightAsync()
        {
            await ProcessSwipeAsync(isLiked: true);
        }

        [RelayCommand]
        private async Task SwipeLeftAsync()
        {
            await ProcessSwipeAsync(isLiked: false);
        }

        private async Task ProcessSwipeAsync(bool isLiked)
        {
            if (CurrentCard == null)
            {
                return;
            }

            var swipedCard = CurrentCard;

            AdvanceToNextCard();

            Task persistTask = _swipeService.UpdatePreferenceScoreAsync(DefaultUserId, swipedCard.MovieId, isLiked);

            await TryRefillQueueAsync(swipedCard.MovieId);

            try
            {
                await persistTask;
            }
            catch (Exception)
            {
            }
        }

        private void AdvanceToNextCard()
        {
            if (CardQueue.Count > 0)
            {
                CurrentCard = CardQueue[0];
                CardQueue.RemoveAt(0);
                IsAllCaughtUp = false;
            }
            else
            {
                CurrentCard = null;
                IsAllCaughtUp = true;
                StatusMessage = "No movies found in database.";
            }
        }

        private async Task TryRefillQueueAsync(int? recentlySwipedMovieId = null)
        {
            if (_isRefilling || CardQueue.Count > RefillThreshold)
            {
                return;
            }

            _isRefilling = true;

            try
            {
                var newMovies = await _swipeService.GetMovieFeedAsync(DefaultUserId, BufferSize);

                var existingIds = new HashSet<int>(CardQueue.Select(m => m.MovieId));
                if (CurrentCard != null)
                {
                    existingIds.Add(CurrentCard.MovieId);
                }

                foreach (var movie in newMovies)
                {
                    if (recentlySwipedMovieId.HasValue && movie.MovieId == recentlySwipedMovieId.Value)
                    {
                        continue;
                    }

                    if (existingIds.Contains(movie.MovieId))
                    {
                        continue;
                    }

                    CardQueue.Add(movie);
                    existingIds.Add(movie.MovieId);
                }

                if (CurrentCard == null && CardQueue.Count > 0)
                {
                    IsAllCaughtUp = false;
                    StatusMessage = "Swipe right to like, left to skip.";
                    AdvanceToNextCard();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _isRefilling = false;
            }
        }
    }
}
