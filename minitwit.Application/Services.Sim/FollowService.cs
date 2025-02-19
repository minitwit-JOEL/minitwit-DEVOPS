using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services.Sim;

public class FollowService : IFollowService
{
    private readonly ApplicationDbContext _dbContext;

    public FollowService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<string>> GetFollowerNames(string username, int no)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            throw new ArgumentException("User not found");
        }
        
        var followerNames = await _dbContext.Followers
            .Where(f => f.WhoId == user.Id)
            .Join(
                _dbContext.Users,
                f => f.WhomId,
                u => u.Id,
                (f, u) => u.Username)
            .Take(no)
            .ToListAsync();

        return followerNames;
    }

    public async Task Follow(string username, string followerName)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        var followUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == followerName);
        if (followUser == null)
        {
            throw new ArgumentException("User not found");
        }
        
        var follow = new Follow { WhoId = user.Id, WhomId = followUser.Id };

        _dbContext.Followers.Add(follow);
        await _dbContext.SaveChangesAsync();
    }

    public async Task Unfollow(string username, string followerName)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            throw new ArgumentException("User not found");
        }
        
        var unfollowUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == followerName);
        if (unfollowUser is null)
        {
            throw new ArgumentException("User not found");
        }

        var follower = await _dbContext.Followers
            .SingleOrDefaultAsync(f => f.WhoId == user.Id && f.WhomId == unfollowUser.Id);
        if (follower is null)
        {
            throw new InvalidOperationException();
        }
        
        _dbContext.Followers.Remove(follower);
        await _dbContext.SaveChangesAsync();
    }
}