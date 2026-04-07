using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace UnitTests.ReelsEditing
{
    [TestFixture]
    public class VideoProcessingServiceTests
    {
        private Mock<IAudioLibraryService> _mockAudioLibrary;
        private VideoProcessingService _service;

        [SetUp]
        public void SetUp()
        {
            _mockAudioLibrary = new Mock<IAudioLibraryService>();
            _service = new VideoProcessingService(_mockAudioLibrary.Object);
        }

        [Test]
        public async Task ApplyCropAsync_VideoFileDoesNotExist_ReturnsOriginalPath()
        {
            string fakePath = "C:\\does_not_exist_video.mp4";
            string cropJson = "{\"x\": 10, \"y\": 10, \"width\": 100, \"height\": 100}";

            string result = await _service.ApplyCropAsync(fakePath, cropJson);

            Assert.That(result, Is.EqualTo(fakePath));
        }

        [Test]
        public async Task ApplyCropAsync_EmptyOrNullJson_ReturnsOriginalPath()
        {
            string existingFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? emptyJson = null;

            string result = await _service.ApplyCropAsync(existingFilePath, emptyJson!);

            // If the JSON is empty, ReadCropData returns the BaseWidth/BaseHeight, 
            // which triggers the early exit returning the original path.
            Assert.That(result, Is.EqualTo(existingFilePath));
        }

        [Test]
        public async Task MergeAudioAsync_VideoFileDoesNotExist_ReturnsOriginalPath()
        {
            string fakePath = "C:\\does_not_exist_video.mp4";

            string result = await _service.MergeAudioAsync(fakePath, 1, 0, 30, 100);

            Assert.That(result, Is.EqualTo(fakePath));
        }
    }
}
