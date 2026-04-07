using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Ubb_se_2026_meio_ai.Core.Database;
using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

namespace UnitTests.ReelsEditing
{
    [TestFixture]
    public class AudioLibraryServiceTests
    {
        private Mock<ISqlConnectionFactory> _mockConnectionFactory;
        private AudioLibraryService _service;

        [SetUp]
        public void SetUp()
        {
            _mockConnectionFactory = new Mock<ISqlConnectionFactory>();
            _service = new AudioLibraryService(_mockConnectionFactory.Object);
        }

        [Test]
        public void GetAllTracksAsync_ConnectionFails_ThrowsException()
        {
            // If the factory fails to create a connection, the service should let that exception bubble up.
            _mockConnectionFactory
                .Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(new InvalidOperationException("Database server is down"));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetAllTracksAsync());

            Assert.That(ex.Message, Is.EqualTo("Database server is down"));
            _mockConnectionFactory.Verify(f => f.CreateConnectionAsync(), Times.Once);
        }

        [Test]
        public void GetTrackByIdAsync_ConnectionFails_ThrowsException()
        {
            _mockConnectionFactory
                .Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(new InvalidOperationException("Network timeout"));

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _service.GetTrackByIdAsync(99));

            Assert.That(ex.Message, Is.EqualTo("Network timeout"));
            _mockConnectionFactory.Verify(f => f.CreateConnectionAsync(), Times.Once);
        }
    }
}
