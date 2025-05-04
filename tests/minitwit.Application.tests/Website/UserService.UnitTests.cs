using Microsoft.EntityFrameworkCore;
using minitwit.Application.Services;
using minitwit.Infrastructure.Data;
using Xunit;

namespace minitwit.Application.UnitTests;

public class UserServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _service;
    
    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
        _service = new UserService(_context);
        
        Util.SeedDatabase(_context);
    }

    [Fact]
    public async Task GetLoggedInUser_ExistingId_ReturnsUser()
    {
        // Arrange
        var existing = _context.Users.First();

        // Act
        var result = await _service.GetLoggedInUser(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Username, result.Username);
    }

    [Fact]
    public async Task GetLoggedInUser_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetLoggedInUser(-1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserById_ExistingId_ReturnsUser()
    {
        // Arrange
        var existing = _context.Users.Last();

        // Act
        var result = await _service.GetUserByIdAsync(existing.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Email, result.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserByIdAsync(-1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsername_ExistingUsername_ReturnsUser()
    {
        // Arrange
        var existing = _context.Users.First();

        // Act
        var result = await _service.GetUserByUsernameAsync(existing.Username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existing.Id, result.Id);
    }

    [Fact]
    public async Task GetUserByUsername_NonExistingUsername_ReturnsNull()
    {
        // Act
        var result = await _service.GetUserByUsernameAsync("no user");

        // Assert
        Assert.Null(result);
    }
}