using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using minitwit.Application.Services.Sim;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;
using Xunit;

namespace minitwit.Application.unit
{
    public class FollowServiceSimTests
    {
        private readonly ApplicationDbContext _context;
        private readonly FollowService _service;

        public FollowServiceSimTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            Util.SeedDatabase(_context);

            var simApiAccess = new SimApiAccess { Key = Environment.GetEnvironmentVariable("SimApiAccess")};
            var opts = Options.Create(simApiAccess);
            var simService = new SimService(_context, opts);

            _service = new FollowService(_context, simService);
        }

        [Fact]
        public async Task GetFollowerNames_ValidUser_ReturnsFollowerNames()
        {
            // Arrange
            const int latestId = 1;
            const string username = "alice";
            const string expectedFollower = "bob";

            // Act
            var names = (await _service.GetFollowerNames(latestId, username, 10)).ToList();
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Single(names);
            Assert.Equal(expectedFollower, names[0]);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task GetFollowerNames_LimitZero_ReturnsEmpty()
        {
            // Arrange
            const int latestId = 2;
            const string username = "alice";

            // Act
            var names = await _service.GetFollowerNames(latestId, username, 0);
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Empty(names);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task GetFollowerNames_InvalidUser_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 3;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetFollowerNames(latestId, "nonexistent", 5));

            // Assert latest was still recorded
            var action = _context.ProcessedActions.Single();
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Follow_NewRelationship_Persists()
        {
            // Arrange
            const int latestId = 4;
            const string follower = "charlie";
            const string followee = "alice";

            // Act
            await _service.Follow(latestId, follower, followee);
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.True(_context.Followers.Any(f =>
                f.WhoId == _context.Users.Single(u => u.Username == follower).Id &&
                f.WhomId == _context.Users.Single(u => u.Username == followee).Id));
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Follow_ExistingRelationship_DoesNotDuplicate()
        {
            // Arrange
            const int latestId = 5;
            var whoId = _context.Users.Single(u => u.Username == "alice").Id;
            var beforeCount = _context.Followers.Count(f => f.WhoId == whoId);

            // Act
            await _service.Follow(latestId, "alice", "bob");
            var afterCount = _context.Followers.Count(f => f.WhoId == whoId);
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Equal(beforeCount, afterCount);
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Follow_InvalidUser_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 6;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Follow(latestId, "no_user", "alice"));

            // Assert
            var action = _context.ProcessedActions.Single();
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Follow_InvalidFollowerName_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 7;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Follow(latestId, "alice", "no_follow"));

            // Assert
            var action = _context.ProcessedActions.Single();
            Assert.Equal(latestId, action.ProcessId);
        }

       
        [Fact]
        public async Task Unfollow_NonExistingRelationship_DoesNothing()
        {
            // Arrange
            const int latestId = 9;
            const string who = "charlie";
            const string whom = "bob";

            // Act
            await _service.Unfollow(latestId, who, whom);
            var action = _context.ProcessedActions.Single();

            // Assert
            Assert.Empty(_context.Followers.Where(f =>
                f.WhoId == _context.Users.Single(u => u.Username == who).Id &&
                f.WhomId == _context.Users.Single(u => u.Username == whom).Id));
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Unfollow_InvalidUser_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 10;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Unfollow(latestId, "no_user", "bob"));

            // Assert
            var action = _context.ProcessedActions.Single();
            Assert.Equal(latestId, action.ProcessId);
        }

        [Fact]
        public async Task Unfollow_InvalidFollowerName_ThrowsArgumentException()
        {
            // Arrange
            const int latestId = 11;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Unfollow(latestId, "alice", "no_follow"));

            // Assert
            var action = _context.ProcessedActions.Single();
            Assert.Equal(latestId, action.ProcessId);
        }
    }
}
