using System;
using System.Collections.Generic;
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
    public class TournamentSetupViewModelTests
    {
        private const int USER_ID = 1;
        private const int MIN_POOL_SIZE = 4;
        private const int BG_COUNT = 4;

        private Mock<ITournamentLogicService> mockedTournamentLogicService = null!;
        private Mock<IMovieTournamentRepository> mockedTournamentRepository = null!;

        [SetUp]
        public void SetUp()
        {
            mockedTournamentLogicService = new Mock<ITournamentLogicService>();
            mockedTournamentRepository = new Mock<IMovieTournamentRepository>();

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MIN_POOL_SIZE);

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(new List<MovieCardModel>());
        }

        [TearDown]
        public void TearDown()
        {
            mockedTournamentLogicService = null!;
            mockedTournamentRepository = null!;
        }

        private async Task<TournamentSetupViewModel> CreateAndWaitAsync(int poolSize = MIN_POOL_SIZE)
        {
            var vm = new TournamentSetupViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            await Task.Delay(50);
            return vm;
        }

        [Test]
        public void Ctor_setsPoolSizeToMinimum()
        {
            var vm = new TournamentSetupViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            Assert.That(vm.PoolSize, Is.EqualTo(MIN_POOL_SIZE));
        }

        [Test]
        public void Ctor_setsSetupErrorMessageToEmpty()
        {
            var vm = new TournamentSetupViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            Assert.That(vm.SetupErrorMessage, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Ctor_triggersLoadSetupDataAsync()
        {
            var vm = new TournamentSetupViewModel(
                mockedTournamentLogicService.Object,
                mockedTournamentRepository.Object);

            mockedTournamentRepository.Verify(
                x => x.GetTournamentPoolSizeAsync(USER_ID),
                Times.Once);
        }

        [Test]
        public async Task LoadSetupDataAsync_setsMaxPoolSize()
        {
            const int MAX_POOL = 16;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.MaxPoolSize, Is.EqualTo(MAX_POOL));
        }

        [Test]
        public async Task LoadSetupDataAsync_fourBackgroundMovies_setsAllFourPosters()
        {
            var movies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, PosterUrl = "http://1.jpg" },
                new MovieCardModel { MovieId = 2, PosterUrl = "http://2.jpg" },
                new MovieCardModel { MovieId = 3, PosterUrl = "http://3.jpg" },
                new MovieCardModel { MovieId = 4, PosterUrl = "http://4.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(movies);

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.BackgroundPoster1, Is.EqualTo("http://1.jpg"));
            Assert.That(vm.BackgroundPoster2, Is.EqualTo("http://2.jpg"));
            Assert.That(vm.BackgroundPoster3, Is.EqualTo("http://3.jpg"));
            Assert.That(vm.BackgroundPoster4, Is.EqualTo("http://4.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_moreThanFourMovies_usesFirstFour()
        {
            var movies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, PosterUrl = "http://1.jpg" },
                new MovieCardModel { MovieId = 2, PosterUrl = "http://2.jpg" },
                new MovieCardModel { MovieId = 3, PosterUrl = "http://3.jpg" },
                new MovieCardModel { MovieId = 4, PosterUrl = "http://4.jpg" },
                new MovieCardModel { MovieId = 5, PosterUrl = "http://5.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(movies);

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.BackgroundPoster1, Is.EqualTo("http://1.jpg"));
            Assert.That(vm.BackgroundPoster4, Is.EqualTo("http://4.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_fewerThanFourMovies_usesAllFallbacks()
        {
            var movies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, PosterUrl = "http://1.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(movies);

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.BackgroundPoster1, Is.EqualTo("https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg"));
            Assert.That(vm.BackgroundPoster2, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/qJ2tW6WMUDux911r6m7haRef0WH.jpg"));
            Assert.That(vm.BackgroundPoster3, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/q2qXg4OmJgm0qGaBYLdXzP8nHPy.jpg"));
            Assert.That(vm.BackgroundPoster4, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/nrmXQ0zcZUL8jFLrakWc90IR8z9.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_emptyMovieList_usesAllFallbacks()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(new List<MovieCardModel>());

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.BackgroundPoster1, Is.EqualTo("https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg"));
            Assert.That(vm.BackgroundPoster2, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/qJ2tW6WMUDux911r6m7haRef0WH.jpg"));
            Assert.That(vm.BackgroundPoster3, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/q2qXg4OmJgm0qGaBYLdXzP8nHPy.jpg"));
            Assert.That(vm.BackgroundPoster4, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/nrmXQ0zcZUL8jFLrakWc90IR8z9.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_exactlyThreeMovies_usesAllFallbacks()
        {
            var movies = new List<MovieCardModel>
            {
                new MovieCardModel { MovieId = 1, PosterUrl = "http://1.jpg" },
                new MovieCardModel { MovieId = 2, PosterUrl = "http://2.jpg" },
                new MovieCardModel { MovieId = 3, PosterUrl = "http://3.jpg" },
            };

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ReturnsAsync(movies);

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.BackgroundPoster1, Is.EqualTo("https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg"));
            Assert.That(vm.BackgroundPoster4, Is.EqualTo("https://media.themoviedb.org/t/p/w600_and_h900_face/nrmXQ0zcZUL8jFLrakWc90IR8z9.jpg"));
        }

        [Test]
        public async Task LoadSetupDataAsync_repositoryThrowsOnPoolSize_setsSetupErrorMessage()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ThrowsAsync(new InvalidOperationException("DB is down"));

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.SetupErrorMessage, Is.Not.Empty.And.Contains("DB is down"));
        }

        [Test]
        public async Task LoadSetupDataAsync_repositoryThrowsOnPool_setsSetupErrorMessage()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolAsync(USER_ID, BG_COUNT))
                .ThrowsAsync(new InvalidOperationException("Pool fetch failed"));

            var vm = await CreateAndWaitAsync();

            Assert.That(vm.SetupErrorMessage, Is.Not.Empty.And.Contains("Pool fetch failed"));
        }

        [Test]
        public async Task LoadSetupDataAsync_repositoryThrows_doesNotThrowToCallerSurface()
        {
            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ThrowsAsync(new Exception("Boom"));

            Assert.DoesNotThrowAsync(async () => await CreateAndWaitAsync());
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeBelowMinimum_setsErrorMessageContainingMinimum()
        {
            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MIN_POOL_SIZE - 1;

            await vm.StartTournamentAsync();

            Assert.That(vm.SetupErrorMessage, Is.Not.Empty.And.Contains(MIN_POOL_SIZE.ToString()));
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeBelowMinimum_doesNotCallService()
        {
            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MIN_POOL_SIZE - 1;

            await vm.StartTournamentAsync();

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeBelowMinimum_doesNotRaiseTournamentStarted()
        {
            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MIN_POOL_SIZE - 1;

            bool eventRaised = false;
            vm.TournamentStarted += (_, _) => eventRaised = true;

            await vm.StartTournamentAsync();

            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeAboveMaximum_setsErrorMessageContainingMaximum()
        {
            const int MAX_POOL = 10;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MAX_POOL + 1;

            await vm.StartTournamentAsync();

            Assert.That(vm.SetupErrorMessage, Is.Not.Empty.And.Contains(MAX_POOL.ToString()));
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeAboveMaximum_doesNotCallService()
        {
            const int MAX_POOL = 10;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MAX_POOL + 1;

            await vm.StartTournamentAsync();

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Test]
        public async Task StartTournamentAsync_poolSizeAboveMaximum_doesNotRaiseTournamentStarted()
        {
            const int MAX_POOL = 10;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MAX_POOL + 1;

            bool eventRaised = false;
            vm.TournamentStarted += (_, _) => eventRaised = true;

            await vm.StartTournamentAsync();

            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public async Task StartTournamentAsync_validPoolSize_callsServiceWithCorrectArguments()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;

            await vm.StartTournamentAsync();

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, GOOD_SIZE),
                Times.Once);
        }

        [Test]
        public async Task StartTournamentAsync_validPoolSize_clearsPreviousErrorMessage()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;
            vm.SetupErrorMessage = "some previous error";

            await vm.StartTournamentAsync();

            Assert.That(vm.SetupErrorMessage, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task StartTournamentAsync_validPoolSize_raisesTournamentStartedEvent()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;

            bool eventRaised = false;
            vm.TournamentStarted += (_, _) => eventRaised = true;

            await vm.StartTournamentAsync();

            Assert.That(eventRaised, Is.True);
        }

        [Test]
        public async Task StartTournamentAsync_validPoolSize_raisesEvent_senderIsViewModel()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;

            object? capturedSender = null;
            vm.TournamentStarted += (sender, _) => capturedSender = sender;

            await vm.StartTournamentAsync();

            Assert.That(capturedSender, Is.SameAs(vm));
        }

        [Test]
        public async Task StartTournamentAsync_exactlyMinPoolSize_isAccepted()
        {
            const int MAX_POOL = 16;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, MIN_POOL_SIZE))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MIN_POOL_SIZE;

            await vm.StartTournamentAsync();

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, MIN_POOL_SIZE),
                Times.Once);
        }

        [Test]
        public async Task StartTournamentAsync_exactlyMaxPoolSize_isAccepted()
        {
            const int MAX_POOL = 16;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, MAX_POOL))
                .Returns(Task.CompletedTask);

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = MAX_POOL;

            await vm.StartTournamentAsync();

            mockedTournamentLogicService.Verify(
                x => x.StartTournamentAsync(USER_ID, MAX_POOL),
                Times.Once);
        }

        [Test]
        public async Task StartTournamentAsync_serviceThrows_setsErrorMessageContainingExceptionMessage()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .ThrowsAsync(new InvalidOperationException("Tournament exploded"));

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;

            await vm.StartTournamentAsync();

            Assert.That(vm.SetupErrorMessage, Is.Not.Empty.And.Contains("Tournament exploded"));
        }

        [Test]
        public async Task StartTournamentAsync_serviceThrows_doesNotRaiseTournamentStarted()
        {
            const int MAX_POOL = 16;
            const int GOOD_SIZE = 8;

            mockedTournamentRepository
                .Setup(x => x.GetTournamentPoolSizeAsync(USER_ID))
                .ReturnsAsync(MAX_POOL);

            mockedTournamentLogicService
                .Setup(x => x.StartTournamentAsync(USER_ID, GOOD_SIZE))
                .ThrowsAsync(new InvalidOperationException("Tournament exploded"));

            var vm = await CreateAndWaitAsync();
            vm.PoolSize = GOOD_SIZE;

            bool eventRaised = false;
            vm.TournamentStarted += (_, _) => eventRaised = true;

            await vm.StartTournamentAsync();

            Assert.That(eventRaised, Is.False);
        }

        [Test]
        public async Task GetImageSource_nullString_returnsNull()
        {
            var vm = await CreateAndWaitAsync();

            Assert.That(vm.GetImageSource(null), Is.Null);
        }

        [Test]
        public async Task GetImageSource_emptyString_returnsNull()
        {
            var vm = await CreateAndWaitAsync();

            Assert.That(vm.GetImageSource(string.Empty), Is.Null);
        }

        [Test]
        public async Task GetImageSource_whitespaceString_returnsNull()
        {
            var vm = await CreateAndWaitAsync();

            Assert.That(vm.GetImageSource("   "), Is.Null);
        }

        [Test]
        public async Task GetImageSource_invalidUri_returnsNull()
        {
            var vm = await CreateAndWaitAsync();

            Assert.That(vm.GetImageSource("not a valid uri"), Is.Null);
        }
    }
}