using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using minitwit.Application.Services.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;
using Xunit;

namespace minitwit.Application.unit;

public class UserServiceSimTests
{
    private readonly ApplicationDbContext _context;
    private readonly SimService _service;
    private readonly string _secretKey;

    public UserServiceSimTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var key = Environment.GetEnvironmentVariable("SimApiAccess");
        _secretKey = key!;
        var simApiAccess = new SimApiAccess { Key = key };
        var opts = Options.Create(simApiAccess);
        
        _service = new SimService(_context, opts);
    }
    
    [Fact]
        public async Task GetLatestProcessedCommandId_NoActions_ReturnsMinusOne()
        {
            // Act
            var latest = await _service.GetLatestProcessedCommandId();

            // Assert
            Assert.Equal(-1, latest);
        }

        [Fact]
        public async Task GetLatestProcessedCommandId_WithActions_ReturnsLatestProcessId()
        {
            // Arrange
            await _context.ProcessedActions.AddAsync(new ProcessedAction { Id = 1, ProcessId = 10 });
            await _context.ProcessedActions.AddAsync(new ProcessedAction { Id = 2, ProcessId = 42 });
            await _context.ProcessedActions.AddAsync(new ProcessedAction { Id = 3, ProcessId = 7 });
            await _context.SaveChangesAsync();

            // Act
            var latest = await _service.GetLatestProcessedCommandId();

            // Assert
            Assert.Equal(7, latest);
        }

        [Fact]
        public async Task UpdateLatest_AddsNewProcessedAction()
        {
            // Arrange
            Assert.Empty(_context.ProcessedActions);

            // Act
            await _service.UpdateLatest(123);
            var all = await _context.ProcessedActions.ToListAsync();

            // Assert
            Assert.Single(all);
            Assert.Equal(123, all[0].ProcessId);
        }

        [Fact]
        public void CheckIfRequestFromSimulator_CorrectHeader_ReturnsTrue()
        {
            // Arrange
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Authorization = _secretKey;

            // Act
            var ok = _service.CheckIfRequestFromSimulator(ctx.Request);

            // Assert
            Assert.True(ok);
        }

        [Fact]
        public void CheckIfRequestFromSimulator_WrongHeader_ReturnsFalse()
        {
            // Arrange
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers.Authorization = "bad-key";

            // Act
            var ok = _service.CheckIfRequestFromSimulator(ctx.Request);

            // Assert
            Assert.False(ok);
        }

        [Fact]
        public void CheckIfRequestFromSimulator_MissingHeader_ReturnsFalse()
        {
            // Arrange
            var ctx = new DefaultHttpContext();

            // Act
            var ok = _service.CheckIfRequestFromSimulator(ctx.Request);

            // Assert
            Assert.False(ok);
        }
}