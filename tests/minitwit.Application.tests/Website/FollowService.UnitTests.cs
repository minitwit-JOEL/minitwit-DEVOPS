using Microsoft.EntityFrameworkCore;
using minitwit.Application.Services;
using minitwit.Infrastructure.Data;
using Xunit;

namespace minitwit.Application.UnitTests;

public class FollowServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly FollowService _service;

    public FollowServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _service = new FollowService(_context);

        _context.Database.EnsureCreated();
        
        Util.SeedDatabase(_context);
    }

    [Fact]
    public async Task CheckFollowByUsername_Existing_ReturnsFollow()
    {
        // Arrange
        var alice = _context.Users.Single(u => u.Username == "alice");

        // Act
        var follow = await _service.CheckFollowByUsername(alice.Id, "bob");

        // Assert
        Assert.NotNull(follow);
        Assert.Equal(alice.Id, follow.WhoId);
        var bob = _context.Users.Single(u => u.Username == "bob");
        Assert.Equal(bob.Id, follow.WhomId);
    }

    [Fact]
    public async Task CheckFollowByUsername_NonExisting_ThrowsInvalidOperationException()
    {
        // Arrange
        var charlie = _context.Users.Single(u => u.Username == "charlie");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckFollowByUsername(charlie.Id, "alice"));
    }

    [Fact]
    public async Task AddFollow_NewRelationship_CreatesFollow()
    {
        // Arrange
        var charlie = _context.Users.Single(u => u.Username == "charlie");

        // Act
        var follow = await _service.AddFollow(charlie.Id, "alice");

        // Assert
        Assert.NotNull(follow);
        Assert.Equal(charlie.Id, follow.WhoId);
        
        var alice = _context.Users.Single(u => u.Username == "alice");
        Assert.Equal(alice.Id, follow.WhomId);
        Assert.True(_context.Followers.Any(f => f.WhoId == charlie.Id && f.WhomId == alice.Id));
    }

    [Theory]
    [InlineData(-1, "alice")]
    [InlineData(1, "nonexistent")] 
    public async Task AddFollow_InvalidUser_ThrowsInvalidOperationException(int whoId, string username)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddFollow(whoId, username));
    }

    [Fact]
    public async Task RemoveFollow_ExistingRelationship_RemovesFollow()
    {
        // Arrange
        var alice = _context.Users.Single(u => u.Username == "alice");

        // Act
        var removed = await _service.RemoveFollow(alice.Id, "bob");

        // Assert
        Assert.NotNull(removed);
        Assert.Equal(alice.Id, removed.WhoId);
        
        var bob = _context.Users.Single(u => u.Username == "bob");
        Assert.Equal(bob.Id, removed.WhomId);
        Assert.False(_context.Followers.Any(f => f.WhoId == alice.Id && f.WhomId == bob.Id));
    }

    [Fact]
    public async Task RemoveFollow_NotFollowing_ThrowsInvalidOperationException()
    {
        // Arrange
        var bob = _context.Users.Single(u => u.Username == "bob");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemoveFollow(bob.Id, "alice"));
    }
}