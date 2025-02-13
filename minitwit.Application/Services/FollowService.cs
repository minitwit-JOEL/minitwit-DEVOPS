using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services;

public class FollowService : IFollowService
{
    public readonly ApplicationDbContext _dbContext;

    public FollowService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Follow> AddFollow(int whoId, string whomUsername)
    {
        var user = await _dbContext.Users.FindAsync(whoId);
        var toFollowUser = await _dbContext.Users.FindAsync(whomUsername);
        
        if (user is null || toFollowUser is null) 
        {
            throw new ArgumentException("No user found with the given username");
        }

        var follow = new Follow { WhoId = whoId, WhomId = whoId };
        
        await _dbContext.AddAsync(follow);
        await _dbContext.SaveChangesAsync();
        
        return follow;
    }

    public async Task<Follow> RemoveFollow(int whoId, string whomUsername)
    {
        var user = await _dbContext.Users.FindAsync(whoId);
        var toUnFollowUser = await _dbContext.Users.FindAsync(whomUsername);
        
        if (user is null || toUnFollowUser is null) 
        {
            throw new ArgumentException("No user found with the given username");
        }

        var follow = await _dbContext.Followers
            .Where(f => f.WhoId == whoId && f.WhomId == toUnFollowUser.Id)
            .FirstAsync();

        if (follow is null)
        {
            throw new ArgumentException("You are currently not following this user");
        }

        _dbContext.Remove(follow);
        await _dbContext.SaveChangesAsync();
        return follow;
    }
}