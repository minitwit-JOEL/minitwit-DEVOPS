using Microsoft.EntityFrameworkCore;
using minitwit.Application.Services;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using Xunit;

namespace minitwit.Application.UnitTests
{
    public class AuthServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _service = new AuthService(_context);
        }

        [Fact]
        public async Task Register_ValidData_CreatesUser()
        {
            // Arrange
            // Act
            var user = await _service.Register(
                username: "testuser",
                email: "test@example.com",
                password: "Password123",
                confirmPassword: "Password123");
            var persisted = _context.Users.Single(u => u.Username == "testuser");
            
            // Assert
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
            Assert.Equal("test@example.com", user.Email);
            Assert.False(string.IsNullOrWhiteSpace(user.Salt));
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123" + user.Salt, user.PasswordHash));
            Assert.Equal(user.Id, persisted.Id);
        }

        [Theory]
        [InlineData("", "email@example.com", "pass", "pass", "You have to enter a username")]
        [InlineData("user", "not-an-email", "pass", "pass", "You have to enter a valid email address")]
        [InlineData("user", "user@example.com", "", "", "You have to enter a password")]
        [InlineData("user", "user@example.com", "pass1", "pass2", "The two passwords do not match")]
        public async Task Register_InvalidInput_ThrowsArgumentException(string username, string email, string pwd, string confirm, string expectedMessage)
        {
            // Arrange

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.Register(username, email, pwd, confirm));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public async Task Register_DuplicateUsername_ThrowsArgumentException()
        {
            // Arrange
            var salt = GenerateSalt();
            var hash = HashPwd("pwd", salt);
            var existing = new User
            {
                Username = "dupuser",
                Email = "dup@example.com",
                Salt = salt,
                PasswordHash = hash
            };
            _context.Users.Add(existing);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.Register("dupuser", "new@example.com", "pwd", "pwd"));
            Assert.Equal("The username is already taken", ex.Message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var salt = GenerateSalt();
            var hash = HashPwd("secret", salt);
            var user = new User
            {
                Username = "loginuser",
                Email = "u@example.com",
                Salt = salt,
                PasswordHash = hash
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.Login("loginuser", "secret");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Theory]
        [InlineData("wronguser", "secret")]
        [InlineData("loginuser", "wrongpass")]
        public async Task Login_InvalidCredentials_ThrowsUnauthorizedAccessException(string username, string password)
        {
            // Arrange
            var salt = GenerateSalt();
            var hash = HashPwd("secret", salt);
            var user = new User
            {
                Username = "loginuser",
                Email = "u@example.com",
                Salt = salt,
                PasswordHash = hash
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.Login(username, password));
        }
        
        private static string GenerateSalt() => BCrypt.Net.BCrypt.GenerateSalt(workFactor: 4);
        private static string HashPwd(string pwd, string salt)
            => BCrypt.Net.BCrypt.HashPassword(pwd + salt, workFactor: 4);
    }
}
