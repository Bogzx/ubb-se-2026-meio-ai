using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Core.Models;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.Services;

namespace UnitTests.TrailerScraping
{
    [TestFixture]
    public class VideoIngestionServiceTests
    {
        private Mock<IYouTubeScraperService> _mockScraper;
        private Mock<IScrapeJobRepository> _mockRepo;
        private Mock<IVideoDownloadService> _mockDownloader;
        private VideoIngestionService _service;

        [SetUp]
        public void SetUp()
        {
            _mockScraper = new Mock<IYouTubeScraperService>();
            _mockRepo = new Mock<IScrapeJobRepository>();
            _mockDownloader = new Mock<IVideoDownloadService>();

            _service = new VideoIngestionService(
                _mockScraper.Object,
                _mockRepo.Object,
                _mockDownloader.Object);
        }

        [Test]
        public async Task IngestVideoFromUrlAsync_ReelAlreadyExists_ReturnsEmptyString()
        {
            _mockRepo.Setup(r => r.ReelExistsByVideoUrlAsync("duplicate_url")).ReturnsAsync(true);

            string result = await _service.IngestVideoFromUrlAsync("duplicate_url", 1);

            Assert.That(result, Is.Empty);
            _mockDownloader.Verify(d => d.DownloadVideoAsMp4Async(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public async Task IngestVideoFromUrlAsync_DownloadFails_ReturnsEmptyString()
        {
            _mockRepo.Setup(r => r.ReelExistsByVideoUrlAsync("new_url")).ReturnsAsync(false);
            _mockDownloader.Setup(d => d.DownloadVideoAsMp4Async("new_url", 60)).ReturnsAsync((string?)null);

            string result = await _service.IngestVideoFromUrlAsync("new_url", 1);

            Assert.That(result, Is.Empty);
            _mockRepo.Verify(r => r.InsertScrapedReelAsync(It.IsAny<ReelModel>()), Times.Never);
        }

        [Test]
        public async Task IngestVideoFromUrlAsync_Success_ReturnsReelId()
        {
            _mockRepo.Setup(r => r.ReelExistsByVideoUrlAsync("new_url")).ReturnsAsync(false);
            _mockDownloader.Setup(d => d.DownloadVideoAsMp4Async("new_url", 60)).ReturnsAsync("C:\\temp\\video.mp4");
            _mockRepo.Setup(r => r.InsertScrapedReelAsync(It.IsAny<ReelModel>())).ReturnsAsync(99);

            string result = await _service.IngestVideoFromUrlAsync("new_url", 1);

            Assert.That(result, Is.EqualTo("99"));
            _mockRepo.Verify(r => r.InsertScrapedReelAsync(It.Is<ReelModel>(m => m.VideoUrl == "C:\\temp\\video.mp4")), Times.Once);
        }
    }
}
