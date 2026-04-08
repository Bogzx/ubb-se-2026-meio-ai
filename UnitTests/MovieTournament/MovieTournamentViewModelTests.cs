using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Microsoft.UI.Xaml.Media.Imaging;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Services;
using ubb_se_2026_meio_ai.Features.MovieTournament.ViewModels;

namespace UnitTests.MovieTournament
{
    [TestFixture]
    public class MovieTournamentViewModelTests
    {
        private const int USER_ID = 1;
        private const int MIN_POOL_SIZE = 4;
        private const int BG_COUNT = 4;

        private Mock<ITournamentLogicService> mockedTournamentLogicService = null!;
        private Mock<IMovieTournamentRepository> mockedTournamentRepository = null!;
        private MovieTournamentViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            mockedTournamentLogicService = new Mock<ITournamentLogicService>();
            mockedTournamentRepository = new Mock<IMovieTournamentRepository>();

            mockedTournamentLogicService
                .SetupGet(x => x.IsTournamentActive)
                .Returns(false);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(0);

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(new List<MovieCardModel>());

            viewModel = new MovieTournamentViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);
        }

        [TearDown]
        public void TearDown()
        {
            mockedTournamentLogicService = null!;
            mockedTournamentRepository = null!;
            viewModel = null!;
        }

        [Test]
        public void Ctor_noActiveTournament_setsViewStateToSetup()
        {
            var vm = new MovieTournamentViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            Assert.That(vm.CurrentViewState, Is.EqualTo(0));
        }

        [Test]
        public void Ctor_tournamentIsActive_setsViewStateToMatchAndUpdateDisplay()
        {
            var match = new MatchPair(
                new MovieCardModel { MovieId = 1, Title = "Match 1" },
                new MovieCardModel { MovieId = 2, Title = "Match 2" });

            var state = new TournamentState();
            state.PendingMatches.Add(match);
            state.CurrentRound = 2;

            mockedTournamentLogicService
                .SetupGet(x => x.IsTournamentActive)
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(state);

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(match);

            var vm = new MovieTournamentViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            Assert.That(vm.CurrentViewState, Is.EqualTo(1));
            Assert.That(vm.MovieOptionA, Is.SameAs(match.FirstMovie));
            Assert.That(vm.MovieOptionB, Is.SameAs(match.SecondMovie));
            Assert.That(vm.RoundDisplay, Is.EqualTo("Round 2"));
        }

        [Test]
        public void Ctor_tournamentIsComplete_setsViewStateToWinnerAndWinnerMovie()
        {
            var winner = new MovieCardModel { MovieId = 1, Title = "Final Winner" };

            mockedTournamentLogicService
                .SetupGet(x => x.IsTournamentActive)
                .Returns(false);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.GetFinalWinner())
                .Returns(winner);

            var vm = new MovieTournamentViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            Assert.That(vm.CurrentViewState, Is.EqualTo(2));
            Assert.That(vm.WinnerMovie, Is.SameAs(winner));
        }

        [Test]
        public async Task LoadSetupDataAsync_loadsMaxPoolSize()
        {
            const int MAX_POOL = 16;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var backgroundMovies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, Title = "BG 1", PosterUrl = "http://1.jpg" },
                new MovieCardModel { MovieId = 2, Title = "BG 2", PosterUrl = "http://2.jpg" },
                new MovieCardModel { MovieId = 3, Title = "BG 3", PosterUrl = "http://3.jpg" },
                new MovieCardModel { MovieId = 4, Title = "BG 4", PosterUrl = "http://4.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(backgroundMovies);

            await viewModel.LoadSetupDataAsync();

            Assert.That(viewModel.MaxPoolSize, Is.EqualTo(MAX_POOL));
            Assert.That(viewModel.BackgroundPoster1, Is.EqualTo("http://1.jpg"));
            Assert.That(viewModel.BackgroundPoster2, Is.EqualTo("http://2.jpg"));
            Assert.That(viewModel.BackgroundPoster3, Is.EqualTo("http://3.jpg"));
            Assert.That(viewModel.BackgroundPoster4, Is.EqualTo("http://4.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_notEnoughBackgroundMovies_usesFallbacksOnlyForMissingSlots()
        {
            const int MAX_POOL = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var backgroundMovies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, Title = "BG 1", PosterUrl = "http://1.jpg" },
                new MovieCardModel { MovieId = 2, Title = "BG 2", PosterUrl = "http://2.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(backgroundMovies);

            await viewModel.LoadSetupDataAsync();

            Assert.That(viewModel.MaxPoolSize, Is.EqualTo(MAX_POOL));
            Assert.That(viewModel.BackgroundPoster1, Is.EqualTo("http://1.jpg"));
            Assert.That(viewModel.BackgroundPoster2, Is.EqualTo("http://2.jpg"));
            Assert.That(viewModel.BackgroundPoster3, Is.Not.Null.And.Contains("themoviedb.org"));
            Assert.That(viewModel.BackgroundPoster4, Is.Not.Null.And.Contains("themoviedb.org"));
        }

        [Test]
        public async Task LoadSetupDataAsync_exception_setsErrorMessage()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ThrowsAsync(new InvalidOperationException("Loading failed"));

            await viewModel.LoadSetupDataAsync();

            Assert.That(viewModel.SetupErrorMessage, Is.Not.Null.And.Contains("Loading failed"));
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeTooSmall_setsErrorMessage()
        {
            const int SMALL_SIZE = 3;

            viewModel.PoolSize = SMALL_SIZE;

            await viewModel.StartTournamentAsync();

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(0));
            Assert.That(viewModel.SetupErrorMessage, Is.Not.Empty);
            Assert.That(viewModel.SetupErrorMessage, Does.Contain(MIN_POOL_SIZE.ToString()));

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, SMALL_SIZE),
                Times.Never);
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeTooLarge_setsErrorMessage()
        {
            const int LARGE_SIZE = 20;

            viewModel.MaxPoolSize = 10;
            viewModel.PoolSize = LARGE_SIZE;

            await viewModel.StartTournamentAsync();

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(0));
            Assert.That(viewModel.SetupErrorMessage, Is.Not.Empty);
            Assert.That(viewModel.SetupErrorMessage, Does.Contain("10"));

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, LARGE_SIZE),
                Times.Never);
        }

        [Test]
        public async Task StartTournamentAsync_validPoolSize_callsService_andTransitionsToMatch()
        {
            const int GOOD_SIZE = 8;

            viewModel.PoolSize = GOOD_SIZE;
            viewModel.MaxPoolSize = 16;

            var match = new MatchPair(
                new MovieCardModel { MovieId = 1, Title = "Movie A" },
                new MovieCardModel { MovieId = 2, Title = "Movie B" });

            var state = new TournamentState();
            state.PendingMatches.Add(match);
            state.CurrentRound = 1;

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .SetupGet(x => x.IsTournamentActive)
                .Returns(true);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(state);

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(match);

            await viewModel.StartTournamentAsync();

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(1));
            Assert.That(viewModel.SetupErrorMessage, Is.Empty);
            Assert.That(viewModel.MovieOptionA, Is.SameAs(match.FirstMovie));
            Assert.That(viewModel.MovieOptionB, Is.SameAs(match.SecondMovie));
            Assert.That(viewModel.RoundDisplay, Is.EqualTo("Round 1"));

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, GOOD_SIZE),
                Times.Once);
        }

        [Test]
        public async Task StartTournamentAsync_serviceThrows_setsErrorMessage()
        {
            const int BAD_SIZE = 8;

            viewModel.PoolSize = BAD_SIZE;
            viewModel.MaxPoolSize = 16;

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, BAD_SIZE))
                .ThrowsAsync(new InvalidOperationException("Service boom"));

            await viewModel.StartTournamentAsync();

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(0));
            Assert.That(viewModel.SetupErrorMessage, Is.Not.Empty);
            Assert.That(viewModel.SetupErrorMessage, Does.Contain("Service boom"));
        }

        [Test]
        public async Task SelectMovieAsync_whenTournamentComplete_setsWinner_andMovesToWinnerState()
        {
            const int WINNER_ID = 1;

            var winner = new MovieCardModel { MovieId = WINNER_ID, Title = "Winner" };

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.GetFinalWinner())
                .Returns(winner);

            viewModel.CurrentViewState = 1;

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(2));
            Assert.That(viewModel.WinnerMovie, Is.SameAs(winner));
        }

        [Test]
        public async Task SelectMovieAsync_tournamentNotComplete_updatesCurrentMatchDisplay()
        {
            const int WINNER_ID = 1;

            var nextMatch = new MatchPair(
                new MovieCardModel { MovieId = 3, Title = "Next A" },
                new MovieCardModel { MovieId = 4, Title = "Next B" });

            var state = new TournamentState();
            state.PendingMatches.Add(nextMatch);
            state.CurrentRound = 2;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(state);

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(nextMatch);

            viewModel.CurrentViewState = 1;

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(1));
            Assert.That(viewModel.MovieOptionA, Is.SameAs(nextMatch.FirstMovie));
            Assert.That(viewModel.MovieOptionB, Is.SameAs(nextMatch.SecondMovie));
            Assert.That(viewModel.RoundDisplay, Is.EqualTo("Round 2"));
        }

        [Test]
        public async Task SelectMovieAsync_tournamentNotComplete_getCurrentMatchReturnsNull_doesNotThrowAndKeepsState()
        {
            const int WINNER_ID = 1;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns((MatchPair?)null);

            viewModel.CurrentViewState = 1;

            Assert.DoesNotThrowAsync(async () => await viewModel.SelectMovieAsync(WINNER_ID));
            Assert.That(viewModel.CurrentViewState, Is.EqualTo(1));
        }

        [Test]
        public void ResetTournament_callsService_andReturnsToSetup()
        {
            viewModel.CurrentViewState = 1;

            viewModel.ResetTournament();

            Assert.That(viewModel.CurrentViewState, Is.EqualTo(0));
            mockedTournamentLogicService.Verify(x => x.ResetTournament(), Times.Once);
        }

        [Test]
        public async Task ResetTournament_triggersLoadSetupDataAsync()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(9);

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(new List<MovieCardModel>
                {
                    new MovieCardModel { MovieId = 1, PosterUrl = "http://1.jpg" },
                    new MovieCardModel { MovieId = 2, PosterUrl = "http://2.jpg" },
                    new MovieCardModel { MovieId = 3, PosterUrl = "http://3.jpg" },
                    new MovieCardModel { MovieId = 4, PosterUrl = "http://4.jpg" }
                });

            viewModel.ResetTournament();

            await Task.Delay(50);

            Assert.That(viewModel.MaxPoolSize, Is.EqualTo(9));
            Assert.That(viewModel.BackgroundPoster1, Is.EqualTo("http://1.jpg"));
        }

        [Test]
        public void GetImageSource_nullString_returnsNull()
        {
            var result = viewModel.GetImageSource(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetImageSource_emptyString_returnsNull()
        {
            var result = viewModel.GetImageSource(string.Empty);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetImageSource_whitespace_returnsNull()
        {
            var result = viewModel.GetImageSource("   ");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetImageSource_invalidUri_returnsNull()
        {
            const string INVALID_URL = "not a uri";

            var result = viewModel.GetImageSource(INVALID_URL);

            Assert.That(result, Is.Null);
        }
    }
}