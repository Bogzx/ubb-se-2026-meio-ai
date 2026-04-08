// <copyright file="ReelsEditingViewModelTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.ReelsEditing
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using Ubb_se_2026_meio_ai.Core.Models;
    using Ubb_se_2026_meio_ai.Features.ReelsEditing.Models;
    using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;
    using Ubb_se_2026_meio_ai.Features.ReelsEditing.ViewModels;

    /// <summary>
    /// Unit tests for the <see cref="ReelsEditingViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class ReelsEditingViewModelTests
    {
        private Mock<IReelRepository> mockRepo;
        private Mock<IVideoProcessingService> mockVideoService;
        private Mock<IAudioLibraryService> mockAudioService;
        private ReelsEditingViewModel viewModel;

        /// <summary>
        /// Sets up the test environment before each test runs.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.mockRepo = new Mock<IReelRepository>();
            this.mockVideoService = new Mock<IVideoProcessingService>();
            this.mockAudioService = new Mock<IAudioLibraryService>();

            this.viewModel = new ReelsEditingViewModel(
                this.mockRepo.Object,
                this.mockVideoService.Object,
                this.mockAudioService.Object);
        }

        /// <summary>
        /// Tests that selecting the crop option triggers the crop mode entered event.
        /// </summary>
        [Test]
        public void SelectEditOptionCommand_SelectsCrop_TriggersCropModeEnteredEvent()
        {
            bool eventTriggered = false;
            this.viewModel.CropModeEntered += () => eventTriggered = true;

            this.viewModel.SelectEditOptionCommand.Execute("Crop");

            Assert.That(this.viewModel.SelectedEditOption, Is.EqualTo("Crop"));
            Assert.That(eventTriggered, Is.True);
        }

        /// <summary>
        /// Tests that applying a valid music selection updates the view model state and status.
        /// </summary>
        [Test]
        public void ApplyMusicSelection_ValidTrack_UpdatesStateAndStatus()
        {
            var track = new MusicTrackModel { MusicTrackId = 1, TrackName = "LoFi Chill" };
            this.viewModel.SelectedReel = new ReelModel { FeatureDurationSeconds = 15.0 };

            this.viewModel.ApplyMusicSelection(track);

            Assert.That(this.viewModel.SelectedMusicTrack, Is.Not.Null);
            Assert.That(this.viewModel.SelectedMusicTrack!.TrackName, Is.EqualTo("LoFi Chill"));
            Assert.That(this.viewModel.IsMusicChosen, Is.True);
            Assert.That(this.viewModel.MusicDuration, Is.EqualTo(15.0)); // Should clamp to reel duration
            Assert.That(this.viewModel.StatusMessage, Is.EqualTo("Music selected: LoFi Chill"));
        }

        /// <summary>
        /// Tests that saving a crop with set margins calculates the correct coordinates and saves successfully.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Test]
        public async Task SaveCropCommand_MarginsSet_CalculatesCorrectCoordinatesAndSaves()
        {
            this.viewModel.SelectedReel = new ReelModel { ReelId = 1, VideoUrl = "original.mp4" };
            this.viewModel.CurrentEdits = new VideoEditMetadata();

            // Set 10% margins on all sides
            this.viewModel.CropMarginLeft = 10;
            this.viewModel.CropMarginTop = 10;
            this.viewModel.CropMarginRight = 10;
            this.viewModel.CropMarginBottom = 10;

            this.mockVideoService
                .Setup(v => v.ApplyCropAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("cropped.mp4");

            this.mockRepo
                .Setup(r => r.UpdateReelEditsAsync(1, It.IsAny<string>(), It.IsAny<int?>(), "cropped.mp4"))
                .ReturnsAsync(1); // Simulate 1 row affected

            this.mockRepo.Setup(r => r.GetReelByIdAsync(1))
                        .ReturnsAsync(() => new ReelModel { ReelId = 1, CropDataJson = this.viewModel.CurrentEdits.ToCropDataJson() });

            await this.viewModel.SaveCropCommand.ExecuteAsync(null);

            // 10% of BaseVideoWidth (1920) = 192
            // 10% of BaseVideoHeight (1080) = 108
            Assert.That(this.viewModel.CurrentEdits.CropXCoordinate, Is.EqualTo(192));
            Assert.That(this.viewModel.CurrentEdits.CropYCoordinate, Is.EqualTo(108));

            // Width should be 80% (100% - 10% left - 10% right) of 1920 = 1536
            Assert.That(this.viewModel.CurrentEdits.CropWidth, Is.EqualTo(1536));

            Assert.That(this.viewModel.IsStatusSuccess, Is.True);
            this.mockRepo.Verify(r => r.UpdateReelEditsAsync(1, It.IsAny<string>(), It.IsAny<int?>(), "cropped.mp4"), Times.Once);
        }

        /// <summary>
        /// Tests that deleting a valid reel calls the repository and goes back to the gallery.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Test]
        public async Task DeleteReelCommand_ValidReel_CallsRepositoryAndGoesBack()
        {
            this.viewModel.SelectedReel = new ReelModel { ReelId = 99 };
            this.viewModel.IsEditing = true;

            await this.viewModel.DeleteReelCommand.ExecuteAsync(null);

            this.mockRepo.Verify(r => r.DeleteReelAsync(99), Times.Once);

            Assert.That(this.viewModel.StatusMessage, Is.EqualTo(string.Empty));

            Assert.That(this.viewModel.SelectedReel, Is.Null);
            Assert.That(this.viewModel.IsEditing, Is.False);
        }

        /// <summary>
        /// Tests that loading a reel populates the correct state and fetches the fresh reel data.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Test]
        public async Task LoadReelAsync_ValidReel_PopulatesStateAndLoadsCropData()
        {
            var reelToLoad = new ReelModel { ReelId = 5, VideoUrl = "old.mp4" };
            var freshReel = new ReelModel { ReelId = 5, VideoUrl = "fresh.mp4", CropDataJson = "{\"x\":0}", BackgroundMusicId = 2 };
            var musicTrack = new MusicTrackModel { MusicTrackId = 2, TrackName = "Background Beat" };

            this.mockRepo.Setup(repository => repository.GetReelByIdAsync(5)).ReturnsAsync(freshReel);
            this.mockAudioService.Setup(audio => audio.GetTrackByIdAsync(2)).ReturnsAsync(musicTrack);

            await this.viewModel.LoadReelAsync(reelToLoad);

            Assert.That(this.viewModel.SelectedReel, Is.Not.Null);
            Assert.That(this.viewModel.SelectedReel!.VideoUrl, Is.EqualTo("fresh.mp4"));
            Assert.That(this.viewModel.IsEditing, Is.True);
            Assert.That(this.viewModel.SelectedMusicTrack, Is.Not.Null);
            Assert.That(this.viewModel.SelectedMusicTrack!.TrackName, Is.EqualTo("Background Beat"));
            Assert.That(this.viewModel.IsMusicChosen, Is.True);
        }

        /// <summary>
        /// Tests that saving music calls the video processing service and updates the database.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Test]
        public async Task SaveMusicAsync_ValidTrack_UpdatesReelAndPersists()
        {
            this.viewModel.SelectedReel = new ReelModel { ReelId = 1, VideoUrl = "cropped.mp4" };
            this.viewModel.SelectedMusicTrack = new MusicTrackModel { MusicTrackId = 3, TrackName = "Jazz" };
            this.viewModel.CurrentEdits = new VideoEditMetadata();

            this.mockVideoService
                .Setup(video => video.MergeAudioAsync("cropped.mp4", 3, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()))
                .ReturnsAsync("final.mp4");

            this.mockRepo
                .Setup(repository => repository.UpdateReelEditsAsync(1, It.IsAny<string>(), 3, "final.mp4"))
                .ReturnsAsync(1);

            this.mockRepo.Setup(repository => repository.GetReelByIdAsync(1))
                        .ReturnsAsync(new ReelModel { ReelId = 1, BackgroundMusicId = 3 });

            await this.viewModel.SaveMusicCommand.ExecuteAsync(null);

            Assert.That(this.viewModel.IsStatusSuccess, Is.True);
            Assert.That(this.viewModel.StatusMessage, Does.Contain("Music selected: Jazz"));
            this.mockRepo.Verify(repository => repository.UpdateReelEditsAsync(1, It.IsAny<string>(), 3, "final.mp4"), Times.Once);
        }
    }
}