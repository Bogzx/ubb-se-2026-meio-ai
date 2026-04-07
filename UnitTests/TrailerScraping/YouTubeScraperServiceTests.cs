using NUnit.Framework;
using Ubb_se_2026_meio_ai.Features.TrailerScraping.Services;

namespace UnitTests.TrailerScraping
{
    [TestFixture]
    public class YouTubeScraperServiceTests
    {
        [Test]
        public void ScrapedVideoResult_VideoUrl_IsFormattedCorrectly()
        {
            var result = new ScrapedVideoResult
            {
                VideoId = "dQw4w9WgXcQ"
            };

            string actualUrl = result.VideoUrl;

            Assert.That(actualUrl, Is.EqualTo("https://www.youtube.com/watch?v=dQw4w9WgXcQ"));
        }

        [Test]
        public void Constructor_AcceptsApiKey()
        {
            string dummyKey = "AIzaSyDummyKeyThatDoesntMatterForThisTest";

            // If the constructor fails or throws, this test will fail.
            var service = new YouTubeScraperService(dummyKey);

            Assert.That(service, Is.Not.Null);
        }
    }
}
