using System.IO;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.Services;

namespace UnitTests.TrailerScraping
{
    [TestFixture]
    public class VideoDownloadServiceTests
    {
        private string _testDownloadFolder;
        private VideoDownloadService _service;

        [SetUp]
        public void SetUp()
        {
            // Use a temporary folder for tests so we don't write to the real app data
            _testDownloadFolder = Path.Combine(Path.GetTempPath(), "MeioAITests", "VideoDownloadService");

            // Clean up any old test runs
            if (Directory.Exists(_testDownloadFolder))
            {
                Directory.Delete(_testDownloadFolder, true);
            }

            _service = new VideoDownloadService(_testDownloadFolder);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temp folder after the test finishes
            if (Directory.Exists(_testDownloadFolder))
            {
                Directory.Delete(_testDownloadFolder, true);
            }
        }

        [Test]
        public void Constructor_CreatesDownloadDirectory()
        {
            Assert.That(Directory.Exists(_testDownloadFolder), Is.True);
        }

        [Test]
        public void GetExpectedFilePath_ReturnsCorrectlyFormattedPath()
        {
            string videoId = "dQw4w9WgXcQ"; // Never gonna give you up
            string expectedPath = Path.Combine(_testDownloadFolder, $"{videoId}.mp4");

            string result = _service.GetExpectedFilePath(videoId);

            Assert.That(result, Is.EqualTo(expectedPath));
        }

        [Test]
        public void LastError_InitiallyNull()
        {
            Assert.That(_service.LastError, Is.Null);
        }
    }
}
