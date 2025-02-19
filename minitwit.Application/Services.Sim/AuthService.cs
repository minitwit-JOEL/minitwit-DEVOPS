using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Services.Sim;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;

    public AuthService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result> Register(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result.Failure("You have to enter a username");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            return Result.Failure("You have to enter a valid email address");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return Result.Failure("You have to enter a password");
        }

        if (await _dbContext.Users.AnyAsync(u => u.Username == username))
        {
            return Result.Failure("The username is already taken");
        }
        
        var user = new User { Username = username, Email = email, PasswordHash = password };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}