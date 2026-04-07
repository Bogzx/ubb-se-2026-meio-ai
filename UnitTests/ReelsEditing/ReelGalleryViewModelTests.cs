using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels;

namespace UnitTests.ReelsEditing
{
    [TestFixture]
    public class ReelGalleryViewModelTests
    {
        private Mock<IReelRepository> _mockRepo;
        private ReelGalleryViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IReelRepository>();
            _viewModel = new ReelGalleryViewModel(_mockRepo.Object);
        }

        [Test]
        public async Task LoadReelsCommand_ReelsExist_PopulatesUserReelsAndSetsFoundMessage()
        {
            var expectedReels = new List<ReelModel>
            {
                new ReelModel { ReelId = 101, Title = "Cluj Coffee Vlog" },
                new ReelModel { ReelId = 102, Title = "Coding Session" }
            };

            _mockRepo.Setup(r => r.GetUserReelsAsync(1)).ReturnsAsync(expectedReels);

            await _viewModel.LoadReelsCommand.ExecuteAsync(null);

            Assert.That(_viewModel.UserReels, Has.Count.EqualTo(2));
            Assert.That(_viewModel.UserReels[0].Title, Is.EqualTo("Cluj Coffee Vlog"));
            Assert.That(_viewModel.IsLoaded, Is.True);
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("2 reel(s) found."));
        }

        [Test]
        public async Task LoadReelsCommand_NoReelsFound_SetsNoReelsMessage()
        {
            _mockRepo.Setup(r => r.GetUserReelsAsync(1)).ReturnsAsync(new List<ReelModel>());

            await _viewModel.LoadReelsCommand.ExecuteAsync(null);

            Assert.That(_viewModel.UserReels, Is.Empty);
            Assert.That(_viewModel.IsLoaded, Is.True);
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("No reels uploaded yet. Upload a reel first."));
        }

        [Test]
        public async Task LoadReelsCommand_RepositoryThrowsException_SetsErrorMessage()
        {
            _mockRepo.Setup(r => r.GetUserReelsAsync(It.IsAny<int>()))
                     .ThrowsAsync(new Exception("Database connection failed"));

            await _viewModel.LoadReelsCommand.ExecuteAsync(null);

            Assert.That(_viewModel.UserReels, Is.Empty);
            Assert.That(_viewModel.StatusMessage, Does.Contain("Error loading reels: Database connection failed"));
            Assert.That(_viewModel.IsLoaded, Is.False);
        }

        [Test]
        public async Task EnsureLoadedAsync_NotLoaded_CallsRepository()
        {
            _viewModel.IsLoaded = false;
            _mockRepo.Setup(r => r.GetUserReelsAsync(It.IsAny<int>())).ReturnsAsync(new List<ReelModel>());

            await _viewModel.EnsureLoadedAsync();

            _mockRepo.Verify(r => r.GetUserReelsAsync(1), Times.Once);
        }

        [Test]
        public async Task EnsureLoadedAsync_AlreadyLoaded_DoesNotCallRepository()
        {
            _viewModel.IsLoaded = true;

            await _viewModel.EnsureLoadedAsync();

            _mockRepo.Verify(r => r.GetUserReelsAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
