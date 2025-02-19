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
    private const int PageSize = 30;


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
            //.Skip(PageSize * page)
            //.Take(PageSize)
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
            //.Skip(PageSize * page)
            //.Take(page)
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

    public async Task<int> GetLatestProcessedCommandId()
    {

        var filePath = "../latest_processed_sim_action_id.txt";

        try
        {
            // Read the file content
            var content = await File.ReadAllTextAsync(filePath);

            content = content.Trim();

            // Try parsing the content to integer
            if (int.TryParse(content, out int latestProcessedCommandId))
            {
                return latestProcessedCommandId;
            }
            else
            {
                Console.Error.WriteLine($"Error: File content '{content}' is not a valid integer.");
                return -1; // Return -1 if the file content is not a valid integer
            }
        }
        catch (Exception e)
        {
            // Return -1 in case of any error, e.g., file not found or invalid format
            Console.Error.WriteLine($"Error: An unexpected exception occurred - {e.Message}");
            return -1;
        }
    }
}