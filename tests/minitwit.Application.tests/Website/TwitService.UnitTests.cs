using Microsoft.EntityFrameworkCore;
using minitwit.Application.Services;
using minitwit.Infrastructure.Data;
using Xunit;

namespace minitwit.Application.UnitTests
{
    public class TwitsServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly TwitsService _service;

        public TwitsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            
            Util.SeedDatabase(_context);

            _service = new TwitsService(_context);
        }

        [Fact]
        public async Task GetPublicTimeline_ReturnsAllOrdered()
        {
            // Act
            var result = await _service.GetPublicTimeline(0);
            var list = result.ToList();

            // Assert
            Assert.Equal(4, list.Count);

            Assert.True(list[0].CreatedAt >= list[1].CreatedAt);
            Assert.True(list[1].CreatedAt >= list[2].CreatedAt);
            Assert.True(list[2].CreatedAt >= list[3].CreatedAt);
        }

        [Fact]
        public async Task GetPublicTimeline_SecondPage_ReturnsEmpty()
        {
            var result = await _service.GetPublicTimeline(1);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFeed_IncludesOwnAndFollowedTwits()
        {
            // Arrange
            var alice = _context.Users.Single(u => u.Username == "alice");

            // Act
            var feed = (await _service.GetFeed(alice.Id, 0)).ToList();

            // Assert
            Assert.Equal(3, feed.Count);
            Assert.Contains(feed, m => m.Author.Username == "alice");
            Assert.Contains(feed, m => m.Author.Username == "bob");
        }

        [Fact]
        public async Task GetFeed_NoFollows_ReturnsOnlyOwnTwits()
        {
            // Arrange
            var charlie = _context.Users.Single(u => u.Username == "charlie");

            // Act
            var feed = await _service.GetFeed(charlie.Id, 0);

            // Assert
            Assert.Single(feed);
            Assert.Equal("charlie", feed.First().Author.Username);
        }

        [Fact]
        public async Task GetUsersTwits_ExistingUser_ReturnsUserTwits()
        {
            // Arrange
            var bob = _context.Users.Single(u => u.Username == "bob");

            // Act
            var twits = await _service.GetUsersTwits(bob.Id, 0);

            // Assert
            Assert.Single(twits);
            Assert.All(twits, m => Assert.Equal(bob.Id, m.AuthorId));
        }

        [Fact]
        public async Task GetUsersTwits_NonExistingUser_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetUsersTwits(userId: -1, 0));
        }

        [Fact]
        public async Task GetUsersTwitsByName_ExistingUser_ReturnsTwits()
        {
            var aliceTwits = await _service.GetUsersTwitsByName("alice", 0);
            Assert.Equal(2, aliceTwits.Count());
            Assert.All(aliceTwits, m => Assert.Equal("alice", m.Author.Username));
        }

        [Fact]
        public async Task GetUsersTwitsByName_NonExistingUser_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.GetUsersTwitsByName("nobody", 0));
        }

        [Fact]
        public async Task PostTwit_ValidText_CreatesAndPersistsMessage()
        {
            // Arrange
            var alice = _context.Users.Single(u => u.Username == "alice");
            const string text = "Unit test posting";

            // Act
            var message = await _service.PostTwit(alice.Id, text);

            // Assert
            Assert.NotNull(message);
            Assert.Equal(alice.Id, message.AuthorId);
            Assert.Equal(text, message.Text);
            Assert.Contains(_context.Messages, m => m.Id == message.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PostTwit_EmptyText_ThrowsArgumentNullException(string invalidText)
        {
            var alice = _context.Users.First();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _service.PostTwit(alice.Id, invalidText));
        }

        [Fact]
        public async Task GetPaginationResponse_ReturnsCorrectMetadata()
        {
            // Act
            var page0 = await _service.GetPaginationResponse(0);

            // Assert
            Assert.Equal(50, page0.PageSize);
            Assert.Equal(4, page0.Total);
            Assert.Equal(1, page0.TotalPages);
            Assert.Equal(0, page0.CurrentPage);
        }
    }
}