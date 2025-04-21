using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services;

public class UserSerivce : IUserService
{
    private readonly ApplicationDbContext _dbContext;

    public UserSerivce(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User?> GetLoggedInUser(int userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    public Task<User> GetUserByIdAsync(int userId)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(u => u!.Username == username);
    }
}