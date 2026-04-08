using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Services;
using ubb_se_2026_meio_ai.Features.MovieTournament.ViewModels;

namespace UnitTests.MovieTournament
{
    [TestFixture]
    public class TournamentMatchViewModelTests
    {
        private const int USER_ID = 1;

        private Mock<ITournamentLogicService> mockedTournamentLogicService = null!;
        private MatchPair defaultMatch = null!;
        private TournamentState defaultState = null!;
        private TournamentMatchViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            mockedTournamentLogicService = new Mock<ITournamentLogicService>();

            defaultMatch = new MatchPair(
                new MovieCardModel { MovieId = 1, Title = "Movie A" },
                new MovieCardModel { MovieId = 2, Title = "Movie B" });

            defaultState = new TournamentState();
            defaultState.PendingMatches.Add(defaultMatch);
            defaultState.CurrentRound = 1;

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(defaultMatch);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(defaultState);

            viewModel = new TournamentMatchViewModel(mockedTournamentLogicService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            mockedTournamentLogicService = null!;
            defaultMatch = null!;
            defaultState = null!;
            viewModel = null!;
        }

        [Test]
        public void Ctor_validMatch_setsMovieOptionA()
        {
            Assert.That(viewModel.MovieOptionA, Is.SameAs(defaultMatch.FirstMovie));
        }

        [Test]
        public void Ctor_validMatch_setsMovieOptionB()
        {
            Assert.That(viewModel.MovieOptionB, Is.SameAs(defaultMatch.SecondMovie));
        }

        [Test]
        public void Ctor_validMatch_setsRoundDisplay()
        {
            Assert.That(viewModel.RoundDisplay, Is.EqualTo("Round 1"));
        }

        [Test]
        public void Ctor_nullCurrentMatch_doesNotThrow_andLeavesPropertiesDefault()
        {
            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns((MatchPair?)null);

            TournamentMatchViewModel vm = null!;
            Assert.DoesNotThrow(() =>
            {
                vm = new TournamentMatchViewModel(mockedTournamentLogicService.Object);
            });

            Assert.That(vm.MovieOptionA, Is.Null);
            Assert.That(vm.MovieOptionB, Is.Null);
            Assert.That(vm.RoundDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void RefreshCurrentMatch_nullCurrentMatch_doesNotUpdateProperties()
        {
            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns((MatchPair?)null);

            viewModel.RefreshCurrentMatch();

            Assert.That(viewModel.MovieOptionA, Is.SameAs(defaultMatch.FirstMovie));
            Assert.That(viewModel.MovieOptionB, Is.SameAs(defaultMatch.SecondMovie));
        }

        [Test]
        public void RefreshCurrentMatch_newMatch_updatesMovieOptionA()
        {
            var newMatch = new MatchPair(
                new MovieCardModel { MovieId = 5, Title = "New A" },
                new MovieCardModel { MovieId = 6, Title = "New B" });

            var newState = new TournamentState();
            newState.PendingMatches.Add(newMatch);
            newState.CurrentRound = 3;

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(newMatch);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(newState);

            viewModel.RefreshCurrentMatch();

            Assert.That(viewModel.MovieOptionA, Is.SameAs(newMatch.FirstMovie));
        }

        [Test]
        public void RefreshCurrentMatch_newMatch_updatesMovieOptionB()
        {
            var newMatch = new MatchPair(
                new MovieCardModel { MovieId = 5, Title = "New A" },
                new MovieCardModel { MovieId = 6, Title = "New B" });

            var newState = new TournamentState();
            newState.PendingMatches.Add(newMatch);
            newState.CurrentRound = 3;

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(newMatch);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(newState);

            viewModel.RefreshCurrentMatch();

            Assert.That(viewModel.MovieOptionB, Is.SameAs(newMatch.SecondMovie));
        }

        [Test]
        public void RefreshCurrentMatch_newMatch_updatesRoundDisplay()
        {
            var newMatch = new MatchPair(
                new MovieCardModel { MovieId = 5, Title = "New A" },
                new MovieCardModel { MovieId = 6, Title = "New B" });

            var newState = new TournamentState();
            newState.PendingMatches.Add(newMatch);
            newState.CurrentRound = 3;

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(newMatch);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(newState);

            viewModel.RefreshCurrentMatch();

            Assert.That(viewModel.RoundDisplay, Is.EqualTo("Round 3"));
        }

        [Test]
        public async Task SelectMovieAsync_callsAdvanceWinnerWithCorrectArguments()
        {
            const int WINNER_ID = 1;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            await viewModel.SelectMovieAsync(WINNER_ID);

            mockedTournamentLogicService.Verify(
                x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID),
                Times.Once);
        }

        [Test]
        public async Task SelectMovieAsync_tournamentNotComplete_refreshesCurrentMatch()
        {
            const int WINNER_ID = 1;

            var nextMatch = new MatchPair(
                new MovieCardModel { MovieId = 3, Title = "Next A" },
                new MovieCardModel { MovieId = 4, Title = "Next B" });

            var nextState = new TournamentState();
            nextState.PendingMatches.Add(nextMatch);
            nextState.CurrentRound = 2;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            mockedTournamentLogicService
                .Setup(x => x.GetCurrentMatch())
                .Returns(nextMatch);

            mockedTournamentLogicService
                .SetupGet(x => x.CurrentState)
                .Returns(nextState);

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(viewModel.MovieOptionA, Is.SameAs(nextMatch.FirstMovie));
            Assert.That(viewModel.MovieOptionB, Is.SameAs(nextMatch.SecondMovie));
            Assert.That(viewModel.RoundDisplay, Is.EqualTo("Round 2"));
        }

        [Test]
        public async Task SelectMovieAsync_tournamentNotComplete_doesNotRaiseTournamentComplete()
        {
            const int WINNER_ID = 1;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            bool eventRaised = false;
            viewModel.TournamentComplete += (_, _) => eventRaised = true;

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public async Task SelectMovieAsync_tournamentComplete_raisesTournamentCompleteEvent()
        {
            const int WINNER_ID = 2;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            bool eventRaised = false;
            viewModel.TournamentComplete += (_, _) => eventRaised = true;

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public async Task SelectMovieAsync_tournamentComplete_doesNotRefreshMatch()
        {
            const int WINNER_ID = 2;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            var movieABeforeSelect = viewModel.MovieOptionA;
            var movieBBeforeSelect = viewModel.MovieOptionB;

            await viewModel.SelectMovieAsync(WINNER_ID);

            mockedTournamentLogicService.Verify(
                x => x.GetCurrentMatch(),
                Times.Once); 
        }

        [Test]
        public async Task SelectMovieAsync_tournamentComplete_senderIsViewModel()
        {
            const int WINNER_ID = 1;

            mockedTournamentLogicService
                .Setup(x => x.AdvanceWinnerAsync(USER_ID, WINNER_ID))
                .Returns(Task.CompletedTask);

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            object? capturedSender = null;
            viewModel.TournamentComplete += (sender, _) => capturedSender = sender;

            await viewModel.SelectMovieAsync(WINNER_ID);

            Assert.That(capturedSender, Is.SameAs(viewModel));
        }

        [Test]
        public void GoBack_callsResetTournament()
        {
            viewModel.GoBack();

            mockedTournamentLogicService.Verify(x => x.ResetTournament(), Times.Once);
        }

        [Test]
        public void GoBack_raisesNavigateBackEvent()
        {
            bool eventRaised = false;
            viewModel.NavigateBack += (_, _) => eventRaised = true;

            viewModel.GoBack();

            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public void GoBack_raisesNavigateBack_senderIsViewModel()
        {
            object? capturedSender = null;
            viewModel.NavigateBack += (sender, _) => capturedSender = sender;

            viewModel.GoBack();

            Assert.That(capturedSender, Is.SameAs(viewModel));
        }

        [Test]
        public void GoBack_resetsBeforeRaisingEvent()
        {
            bool resetCalledBeforeEvent = false;

            viewModel.NavigateBack += (_, _) =>
            {
                try
                {
                    mockedTournamentLogicService.Verify(x => x.ResetTournament(), Times.Once);
                    resetCalledBeforeEvent = true;
                }
                catch
                {
                    resetCalledBeforeEvent = false;
                }
            };

            viewModel.GoBack();

            Assert.That(resetCalledBeforeEvent, Is.True);
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
        public void GetImageSource_whitespaceString_returnsNull()
        {
            var result = viewModel.GetImageSource("   ");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetImageSource_invalidUri_returnsNull()
        {
            const string INVALID_URL = "not a valid uri";

            var result = viewModel.GetImageSource(INVALID_URL);

            Assert.That(result, Is.Null);
        }

    }
}