using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Models;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels;

namespace UnitTests.ReelsEditing
{
    [TestFixture]
    public class ReelsEditingViewModelTests
    {
        private Mock<IReelRepository> _mockRepo;
        private Mock<IVideoProcessingService> _mockVideoService;
        private Mock<IAudioLibraryService> _mockAudioService;
        private ReelsEditingViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _mockRepo = new Mock<IReelRepository>();
            _mockVideoService = new Mock<IVideoProcessingService>();
            _mockAudioService = new Mock<IAudioLibraryService>();

            _viewModel = new ReelsEditingViewModel(
                _mockRepo.Object,
                _mockVideoService.Object,
                _mockAudioService.Object);
        }

        [Test]
        public void SelectEditOptionCommand_SelectsCrop_TriggersCropModeEnteredEvent()
        {
            bool eventTriggered = false;
            _viewModel.CropModeEntered += () => eventTriggered = true;

            _viewModel.SelectEditOptionCommand.Execute("Crop");

            Assert.That(_viewModel.SelectedEditOption, Is.EqualTo("Crop"));
            Assert.That(eventTriggered, Is.True);
        }

        [Test]
        public void ApplyMusicSelection_ValidTrack_UpdatesStateAndStatus()
        {
            var track = new MusicTrackModel { MusicTrackId = 1, TrackName = "LoFi Chill" };
            _viewModel.SelectedReel = new ReelModel { FeatureDurationSeconds = 15.0 };

            _viewModel.ApplyMusicSelection(track);

            Assert.That(_viewModel.SelectedMusicTrack, Is.Not.Null);
            Assert.That(_viewModel.SelectedMusicTrack!.TrackName, Is.EqualTo("LoFi Chill"));
            Assert.That(_viewModel.IsMusicChosen, Is.True);
            Assert.That(_viewModel.MusicDuration, Is.EqualTo(15.0)); // Should clamp to reel duration
            Assert.That(_viewModel.StatusMessage, Is.EqualTo("Music selected: LoFi Chill"));
        }

        [Test]
        public async Task SaveCropCommand_MarginsSet_CalculatesCorrectCoordinatesAndSaves()
        {
            _viewModel.SelectedReel = new ReelModel { ReelId = 1, VideoUrl = "original.mp4" };
            _viewModel.CurrentEdits = new VideoEditMetadata();

            // Set 10% margins on all sides
            _viewModel.CropMarginLeft = 10;
            _viewModel.CropMarginTop = 10;
            _viewModel.CropMarginRight = 10;
            _viewModel.CropMarginBottom = 10;

            _mockVideoService
                .Setup(v => v.ApplyCropAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("cropped.mp4");

            _mockRepo
                .Setup(r => r.UpdateReelEditsAsync(1, It.IsAny<string>(), It.IsAny<int?>(), "cropped.mp4"))
                .ReturnsAsync(1); // Simulate 1 row affected

            _mockRepo.Setup(r => r.GetReelByIdAsync(1))
                        .ReturnsAsync(() => new ReelModel { ReelId = 1, CropDataJson = _viewModel.CurrentEdits.ToCropDataJson() });

            await _viewModel.SaveCropCommand.ExecuteAsync(null);

            // 10% of BaseVideoWidth (1920) = 192
            // 10% of BaseVideoHeight (1080) = 108
            Assert.That(_viewModel.CurrentEdits.CropXCoordinate, Is.EqualTo(192));
            Assert.That(_viewModel.CurrentEdits.CropYCoordinate, Is.EqualTo(108));

            // Width should be 80% (100% - 10% left - 10% right) of 1920 = 1536
            Assert.That(_viewModel.CurrentEdits.CropWidth, Is.EqualTo(1536));

            Assert.That(_viewModel.IsStatusSuccess, Is.True);
            _mockRepo.Verify(r => r.UpdateReelEditsAsync(1, It.IsAny<string>(), It.IsAny<int?>(), "cropped.mp4"), Times.Once);
        }

        [Test]
        public async Task DeleteReelCommand_ValidReel_CallsRepositoryAndGoesBack()
        {
            _viewModel.SelectedReel = new ReelModel { ReelId = 99 };
            _viewModel.IsEditing = true;

            await _viewModel.DeleteReelCommand.ExecuteAsync(null);

            _mockRepo.Verify(r => r.DeleteReelAsync(99), Times.Once);

            Assert.That(_viewModel.StatusMessage, Is.EqualTo(string.Empty));

            Assert.That(_viewModel.SelectedReel, Is.Null);
            Assert.That(_viewModel.IsEditing, Is.False);
        }
    }
}