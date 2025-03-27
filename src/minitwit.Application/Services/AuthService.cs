using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> Login(string username, string password)
    {
        var user = await _dbContext.Users
            .Where(u => u.Username == username)
            .FirstAsync();
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password + user.Salt, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        return user;
    }

    public async Task<User?> Register(string username, string email, string password, string confirmPassword)
    {
        string? error = null;

        if (string.IsNullOrWhiteSpace(username))
        {
            error = "You have to enter a username";
        }
        else if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            error = "You have to enter a valid email address";
        }
        else if (string.IsNullOrWhiteSpace(password))
        {
            error = "You have to enter a password";
        }
        else if (password != confirmPassword)
        {
            error = "The two passwords do not match";
        }
        else if (await _dbContext.Users.AnyAsync(u => u.Username == username))
        {
            error = "The username is already taken";
        }

        if (error != null)
        {
            throw new ArgumentException(error);
        }
        
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password + salt, 10);

        var user = new User { Username = username, Email = email, PasswordHash = hashedPassword, Salt = salt };
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }
}