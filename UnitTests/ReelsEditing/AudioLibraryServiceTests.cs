// <copyright file="AudioLibraryServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UnitTests.ReelsEditing
{
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Moq;
    using NUnit.Framework;
    using Ubb_se_2026_meio_ai.Core.Database;
    using Ubb_se_2026_meio_ai.Features.ReelsEditing.Services;

    /// <summary>
    /// Unit tests for the <see cref="AudioLibraryService"/> class.
    /// </summary>
    [TestFixture]
    public class AudioLibraryServiceTests
    {
        private Mock<ISqlConnectionFactory> mockConnectionFactory;
        private AudioLibraryService service;

        /// <summary>
        /// Sets up the test environment before each test runs.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.mockConnectionFactory = new Mock<ISqlConnectionFactory>();
            this.service = new AudioLibraryService(this.mockConnectionFactory.Object);
        }

        /// <summary>
        /// Tests that an exception is thrown when the database connection fails during GetAllTracksAsync.
        /// </summary>
        [Test]
        public void GetAllTracksAsync_ConnectionFails_ThrowsException()
        {
            // If the factory fails to create a connection, the service should let that exception bubble up.
            this.mockConnectionFactory
                .Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(new InvalidOperationException("Database server is down"));

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await this.service.GetAllTracksAsync());

            Assert.That(exception.Message, Is.EqualTo("Database server is down"));
            this.mockConnectionFactory.Verify(f => f.CreateConnectionAsync(), Times.Once);
        }

        /// <summary>
        /// Tests that an exception is thrown when the database connection fails during GetTrackByIdAsync.
        /// </summary>
        [Test]
        public void GetTrackByIdAsync_ConnectionFails_ThrowsException()
        {
            this.mockConnectionFactory
                .Setup(f => f.CreateConnectionAsync())
                .ThrowsAsync(new InvalidOperationException("Network timeout"));

            var exception = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await this.service.GetTrackByIdAsync(99));

            Assert.That(exception.Message, Is.EqualTo("Network timeout"));
            this.mockConnectionFactory.Verify(f => f.CreateConnectionAsync(), Times.Once);
        }
    }
}