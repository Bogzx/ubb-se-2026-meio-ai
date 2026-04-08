using System;
using Moq;
using NUnit.Framework;
using ubb_se_2026_meio_ai.Core.Models;
using ubb_se_2026_meio_ai.Features.MovieTournament.Services;
using ubb_se_2026_meio_ai.Features.MovieTournament.ViewModels;

namespace UnitTests.MovieTournament
{
    [TestFixture]
    public class TournamentWinnerViewModelTests
    {
        private Mock<ITournamentLogicService> mockedTournamentLogicService = null!;

        [SetUp]
        public void SetUp()
        {
            mockedTournamentLogicService = new Mock<ITournamentLogicService>();

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);
        }

        [TearDown]
        public void TearDown()
        {
            mockedTournamentLogicService = null!;
        }

        [Test]
        public void Ctor_tournamentNotComplete_winnerMovieIsNull()
        {
            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.WinnerMovie, Is.Null);
        }

        [Test]
        public void Ctor_tournamentNotComplete_doesNotCallGetFinalWinner()
        {
            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(false);

            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            mockedTournamentLogicService.Verify(
                x => x.GetFinalWinner(),
                Times.Never);
        }

        [Test]
        public void Ctor_tournamentComplete_setsWinnerMovie()
        {
            var winner = new MovieCardModel { MovieId = 1, Title = "The Winner" };

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.GetFinalWinner())
                .Returns(winner);

            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.WinnerMovie, Is.SameAs(winner));
        }

        [Test]
        public void Ctor_tournamentComplete_callsGetFinalWinnerOnce()
        {
            var winner = new MovieCardModel { MovieId = 1, Title = "The Winner" };

            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.GetFinalWinner())
                .Returns(winner);

            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            mockedTournamentLogicService.Verify(
                x => x.GetFinalWinner(),
                Times.Once);
        }

        [Test]
        public void Ctor_tournamentComplete_getFinalWinnerReturnsNull_winnerMovieIsNull()
        {
            mockedTournamentLogicService
                .Setup(x => x.IsTournamentComplete())
                .Returns(true);

            mockedTournamentLogicService
                .Setup(x => x.GetFinalWinner())
                .Returns((MovieCardModel?)null);

            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.WinnerMovie, Is.Null);
        }

        [Test]
        public void StartAnotherTournament_callsResetTournament()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            vm.StartAnotherTournament();

            mockedTournamentLogicService.Verify(x => x.ResetTournament(), Times.Once);
        }

        [Test]
        public void StartAnotherTournament_raisesNavigateToSetupEvent()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            bool eventRaised = false;
            vm.NavigateToSetup += (_, _) => eventRaised = true;

            vm.StartAnotherTournament();

            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public void StartAnotherTournament_raisesNavigateToSetup_senderIsViewModel()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            object? capturedSender = null;
            vm.NavigateToSetup += (sender, _) => capturedSender = sender;

            vm.StartAnotherTournament();

            Assert.That(capturedSender, Is.SameAs(vm));
        }

        [Test]
        public void StartAnotherTournament_resetsBeforeRaisingEvent()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            bool resetCalledBeforeEvent = false;
            vm.NavigateToSetup += (_, _) =>
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

            vm.StartAnotherTournament();

            Assert.That(resetCalledBeforeEvent, Is.True);
        }

        [Test]
        public void StartAnotherTournament_noSubscribers_doesNotThrow()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.DoesNotThrow(() => vm.StartAnotherTournament());
        }

        [Test]
        public void GetImageSource_nullString_returnsNull()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.GetImageSource(null), Is.Null);
        }

        [Test]
        public void GetImageSource_emptyString_returnsNull()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.GetImageSource(string.Empty), Is.Null);
        }

        [Test]
        public void GetImageSource_whitespaceString_returnsNull()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.GetImageSource("   "), Is.Null);
        }

        [Test]
        public void GetImageSource_invalidUri_returnsNull()
        {
            var vm = new TournamentWinnerViewModel(mockedTournamentLogicService.Object);

            Assert.That(vm.GetImageSource("not a valid uri"), Is.Null);
        }
    }
}