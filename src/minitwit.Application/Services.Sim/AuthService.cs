using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Services.Sim;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISimService _simService;

    public AuthService(ApplicationDbContext dbContext, ISimService simService)
    {
        _dbContext = dbContext;
        _simService = simService;
    }
    
    public async Task<Result> Register(int latest, string username, string email, string password)
    {
        await _simService.UpdateLatest(latest);
        
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
        
        var user = new User { Username = username, Email = email, PasswordHash = password, Salt = "" };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        return Result.Success();
    }
}