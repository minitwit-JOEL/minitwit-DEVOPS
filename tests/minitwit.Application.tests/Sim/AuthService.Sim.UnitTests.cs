using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using minitwit.Application.Services.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;
using Xunit;

namespace minitwit.Application.unit;

public class AuthServiceSimTests
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _service;

    public AuthServiceSimTests()
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
        _service = new AuthService(_context, simService);
    }
    
    [Fact]
    public async Task Register_ValidInputs_PersistsUserAndProcessedAction()
    {
        // Arrange
        // Act
        var result = await _service.Register(
            latest: 42,
            username: "user1",
            email: "user1@example.com",
            password: "pwd1");
        var action = _context.ProcessedActions.Single();
        var user = _context.Users.Single(u => u.Username == "user1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.ErrorMessage);
        Assert.Equal("user1@example.com", user.Email);
        Assert.Equal(42, action.ProcessId);
    }

    [Theory]
    [InlineData("", "e@e.com", "pwd", "You have to enter a username")]
    [InlineData("user2", "not-an-email", "pwd", "You have to enter a valid email address")]
    [InlineData("user3", "u3@e.com", "", "You have to enter a password")]
    public async Task Register_InvalidInputs_ReturnsFailure_DoesNotPersist(
        string username,
        string email,
        string password,
        string expectedError)
    {
        // Arrange
        // Act
        var result = await _service.Register(
            latest: 100,
            username: username,
            email: email,
            password: password);
        var action = _context.ProcessedActions.Single();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError, result.ErrorMessage);
        Assert.Empty(_context.Users.Where(u => u.Username == username));
        Assert.Equal(100, action.ProcessId);
    }

    [Fact]
    public async Task Register_DuplicateUsername_ReturnsFailure_AndDoesNotAddSecondUser()
    {
        // Arrange
        _context.Users.Add(new User { Username = "dup", Email = "dup@e.com", PasswordHash = "p", Salt = "" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.Register(
            latest: 7,
            username: "dup",
            email: "dup2@e.com",
            password: "pwd");
        var action = _context.ProcessedActions.Single();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("The username is already taken", result.ErrorMessage);
        Assert.Equal(1, _context.Users.Count(u => u.Username == "dup"));
        Assert.Equal(7, action.ProcessId);
    }
}