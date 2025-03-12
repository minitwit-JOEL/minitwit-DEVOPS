using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;

namespace minitwit.Application.Services.Sim;

public class FollowService : IFollowService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISimService _simService;

    public FollowService(ApplicationDbContext dbContext, ISimService simService)
    {
        _dbContext = dbContext;
        _simService = simService;
    }

    public async Task<IEnumerable<string>> GetFollowerNames(int latest, string username, int no)
    {
        await _simService.UpdateLatest(latest);
        
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

    public async Task Follow(int latest, string username, string followerName)
    {
        await _simService.UpdateLatest(latest);
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }
        
        var followUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == followerName);
        if (followUser == null)
        {
            throw new InvalidOperationException();
        }

        var existingEntity = await _dbContext.Followers
            .Where(f => f.WhoId == user.Id && f.WhomId == followUser.Id)
            .FirstOrDefaultAsync();

        if (existingEntity == null) {
            var follow = new Follow { WhoId = user.Id, WhomId = followUser.Id };
            await _dbContext.AddAsync(follow);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task Unfollow(int latest, string username, string followerName)
    {
        await _simService.UpdateLatest(latest);
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user is null)
        {
            throw new ArgumentException("User not found");
        }
        
        var unfollowUser = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == followerName);
        if (unfollowUser is null)
        {
            throw new InvalidOperationException();
        }

        var follower = await _dbContext.Followers
            .Where(f => f.WhoId == user.Id && f.WhomId == unfollowUser.Id)
            .Select(f => new Follow {
                Id = f.Id,
                WhoId = f.WhoId,
                WhomId = f.WhomId
            })
            .ToListAsync();
        
        _dbContext.Followers.RemoveRange(follower);
        await _dbContext.SaveChangesAsync();            
    }
}