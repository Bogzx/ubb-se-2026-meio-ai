using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.Services;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.ViewModels;

namespace UnitTests.TrailerScraping
{
    [TestFixture]
    public class TrailerScrapingViewModelTests
    {
        private Mock<IScrapeJobRepository> _mockRepo;
        private Mock<IVideoIngestionService> _mockIngestionService;
        private TrailerScrapingViewModel _viewModel;
        
        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IScrapeJobRepository>();
            _mockIngestionService = new Mock<IVideoIngestionService>(); // No constructor args needed!

            _viewModel = new TrailerScrapingViewModel(_mockIngestionService.Object, _mockRepo.Object);
        }

        [Test]
        public async Task SearchMoviesCommand_QueryTooShort_ClearsSuggestions()
        {
            _viewModel.SuggestedMovies.Add(new MovieCardModel());

            await _viewModel.SearchMoviesCommand.ExecuteAsync("A");

            Assert.That(_viewModel.SuggestedMovies, Is.Empty);
            _mockRepo.Verify(r => r.SearchMoviesByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SearchMoviesCommand_ValidQuery_PopulatesSuggestions()
        {
            var mockResults = new List<MovieCardModel>
            {
                new MovieCardModel { Title = "Inception" },
                new MovieCardModel { Title = "Interstellar" }
            };

            _mockRepo.Setup(r => r.SearchMoviesByNameAsync("Inc"))
                     .ReturnsAsync(mockResults);

            await _viewModel.SearchMoviesCommand.ExecuteAsync("Inc");

            Assert.That(_viewModel.SuggestedMovies.Count, Is.EqualTo(2));
            Assert.That(_viewModel.NoMovieFound, Is.False);
        }

        [Test]
        public void SelectMovie_ValidMovie_UpdatesPropertiesAndCommands()
        {
            var selectedMovie = new MovieCardModel { Title = "The Matrix" };

            _viewModel.SelectMovie(selectedMovie);

            Assert.That(_viewModel.SelectedMovie, Is.EqualTo(selectedMovie));
            Assert.That(_viewModel.SearchText, Is.EqualTo("The Matrix"));
            Assert.That(_viewModel.StartScrapeCommand.CanExecute(null), Is.True);
        }

        [Test]
        public async Task StartScrapeCommand_ValidMovie_CallsIngestionService()
        {
            var selectedMovie = new MovieCardModel { Title = "Dune" };
            _viewModel.SelectMovie(selectedMovie);
            _viewModel.MaxResults = 10;

            _mockIngestionService
                .Setup(s => s.RunScrapeJobAsync(selectedMovie, 10, It.IsAny<Func<ScrapeJobLogModel, Task>>()))
                .ReturnsAsync(new ScrapeJobModel());

            // Dummy stats to prevent null references during the finally block's RefreshAsync
            _mockRepo.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync(new DashboardStatsModel());

            await _viewModel.StartScrapeCommand.ExecuteAsync(null);

            _mockIngestionService.Verify(
                s => s.RunScrapeJobAsync(selectedMovie, 10, It.IsAny<Func<ScrapeJobLogModel, Task>>()),
                Times.Once);

            // Verify state is reset in 'finally' block
            Assert.That(_viewModel.IsScraping, Is.False);
            Assert.That(_viewModel.StatusText, Is.EqualTo("Idle"));
        }

        [Test]
        public async Task RefreshCommand_UpdatesDashboardStats()
        {
            var stats = new DashboardStatsModel
            {
                TotalMovies = 100,
                TotalReels = 50,
                TotalJobs = 10,
                RunningJobs = 2,
                CompletedJobs = 7,
                FailedJobs = 1
            };

            _mockRepo.Setup(r => r.GetDashboardStatsAsync()).ReturnsAsync(stats);
            _mockRepo.Setup(r => r.GetAllLogsAsync()).ReturnsAsync(new List<ScrapeJobLogModel>());
            _mockRepo.Setup(r => r.GetAllMoviesAsync()).ReturnsAsync(new List<MovieCardModel>());
            _mockRepo.Setup(r => r.GetAllReelsAsync()).ReturnsAsync(new List<ReelModel>());

            await _viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(_viewModel.TotalMovies, Is.EqualTo(100));
            Assert.That(_viewModel.TotalReels, Is.EqualTo(50));
            Assert.That(_viewModel.RunningJobs, Is.EqualTo(2));
            Assert.That(_viewModel.FailedJobs, Is.EqualTo(1));
        }
    }
}
