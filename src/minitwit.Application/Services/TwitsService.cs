using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using minitwit.Application.Interfaces;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos;

namespace minitwit.Application.Services;

public class TwitsService : ITwitsService
{
    private readonly ApplicationDbContext _dbContext;
    private const int PageSize = 50;


    public TwitsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Message>> GetPublicTimeline(int page)
    {
        return await _dbContext.Messages
            .Include(m => m.Author)
            .Where(m => !m.Flagged)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(PageSize * page)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetFeed(int userId, int page)
    {
        var followers = await _dbContext.Followers
            .Where(f => f.WhoId == userId)
            .Select(f => f.WhomId)
            .ToListAsync();

        return await _dbContext.Messages
            .Include(m => m.Author)
            .Where(m => !m.Flagged && (m.AuthorId == userId || followers.Contains(m.AuthorId)))
            .OrderByDescending(m => m.CreatedAt)
            .Skip(PageSize * page)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetUsersTwits(int userId, int page = default)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user is null)
        {
            throw new ArgumentException("No user with this id");
        }

        var twits = await _dbContext.Messages
            .Include(m => m.Author)
            .Where(m => m.AuthorId == user.Id && !m.Flagged)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(PageSize * page)
            .Take(page)
            .ToListAsync();

        return twits;
    }

    public async Task<IEnumerable<Message>> GetUsersTwitsByName(string userName, int page = default)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Username == userName);
        if (user is null)
        {
            throw new ArgumentException("No user with this name");
        }

        var twits = await _dbContext.Messages
            .Include(m => m.Author)
            .Where(m => m.Author.Username == user.Username && !m.Flagged)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(PageSize * page)
            .Take(page)
            .ToListAsync();

        return twits;
    }

    public async Task<Message> PostTwit(int userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException("Message cannot be empty");
        }

        var newMessage = new Message
        {
            AuthorId = userId,
            Text = text,
            CreatedAt = DateTime.UtcNow,
            Flagged = false
        };

        await _dbContext.Messages.AddAsync(newMessage);
        await _dbContext.SaveChangesAsync();
        return newMessage;
    }

    public async Task<PaginationResponse> GetPaginationResponse(int page)
    {
        var total = await _dbContext.Messages.CountAsync();

        return new PaginationResponse
        {
            PageSize = PageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)PageSize),
            CurrentPage = page
        };
    }
}