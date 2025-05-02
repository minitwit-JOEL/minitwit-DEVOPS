using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using minitwit.Application.Services.Sim;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;
using Xunit;

namespace minitwit.Application.unit;

public class TwitServiceSimTests
{
    private readonly ApplicationDbContext _context;
    private readonly TwitsService _service;

    public TwitServiceSimTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        Util.SeedDatabase(_context);

        var simApiAccess = new SimApiAccess { Key = Environment.GetEnvironmentVariable("SimApiAccess") };
        var opts = Options.Create(simApiAccess);
        var simService = new SimService(_context, opts);

        
        _service = new TwitsService(_context, simService);
    }
    
     [Fact]
        public async Task GetMessages_ReturnsDescendingAndRespectsLimit()
        {
            // Arrange
            const int latestId = 1;
            const int limit = 1;

            // Act
            var result = (await _service.GetMessages(latestId, limit)).ToList();
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Single(result);
            Assert.Equal("Good morning, world!", result[0].Content);
            Assert.Equal("charlie", result[0].User);
            Assert.Single(result);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task GetMessages_LimitZero_ReturnsEmpty()
        {
            // Arrange
            const int latestId = 2;

            // Act
            var result = await _service.GetMessages(latestId, limit: 0);
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Empty(result);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task GetMessagesForValidUser_ReturnsTheirMessages()
        {
            // Arrange
            const int latestId = 3;
            const string username = "alice";
            const int limit = 5;

            // Act
            var result = (await _service.GetMessagesForUser(latestId, username, limit)).ToList();
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Another thought...", result[0].Content);
            Assert.Equal("Hello from Alice!", result[1].Content);
            Assert.Equal("alice", result[0].User);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task GetMessagesForInvalidUser_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 4;
            const string username = "dorothy";

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetMessagesForUser(latestId, username, 10));
        }

        [Fact]
        public async Task PostMessagesForValidUser_AddsMessage()
        {
            // Arrange
            const int latestId = 5;
            const string username = "bob";
            const string content = "new bob message";
            var beforeCount = _context.Messages.Count();

            // Act
            await _service.PostMessagesForUser(latestId, username, content);
            var action = _context.ProcessedActions.Single();
            var all = _context.Messages.Include(m => m.Author).ToList();
            var inserted = all.OrderByDescending(m => m.CreatedAt).First();

            // Assert
            Assert.Equal(beforeCount + 1, all.Count);
            Assert.Equal(content, inserted.Text);
            Assert.False(inserted.Flagged);
            Assert.Equal(username, _context.Users.Find(inserted.AuthorId)?.Username);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task PostMessagesForUser_InvalidUser_DoesNothingAndUpdatesLatest()
        {
            // Arrange
            const int latestId = 6;
            const string username = "ghost";
            var beforeCount = _context.Messages.Count();

            // Act
            await _service.PostMessagesForUser(latestId, username, "any");
            var action = _context.ProcessedActions.Single();
            var afterCount = _context.Messages.Count();

            // Assert
            Assert.Equal(beforeCount, afterCount);
            Assert.Equal(latestId, action.ProcessId);
        }
}