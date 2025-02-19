using Microsoft.EntityFrameworkCore;
using minitwit.Application.Interfaces.Sim;
using minitwit.Domain.Entities;
using minitwit.Infrastructure.Data;
using minitwit.Infrastructure.Dtos;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Services.Sim;

public class TwitsService : ITwitsService
{
    private readonly ApplicationDbContext _dbContext;

    public TwitsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IEnumerable<MessageDto>> GetMessages(int limit = 100)
    {
        var messages = await _dbContext.Messages
            .Where(m => !m.Flagged)
            .Include(m => m.Author)
            .OrderByDescending(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new MessageDto
            {
                Content = m.Text,
                PubDate = m.CreatedAt,
                User = m.Author.Username
            })
            .ToListAsync();

        return messages;
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesForUser(string username, int no)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Username == username);
        if (user is null)
        {
            throw new ArgumentException("User not found");
        }

        var messages = await _dbContext.Messages
            .Include(m => m.Author)
            .Where(m => m.AuthorId == user.Id && !m.Flagged)
            .OrderByDescending(m => m.CreatedAt)
            .Take(no)
            .Select(m => new MessageDto
            {
                Content = m.Text,
                PubDate = m.CreatedAt,
                User = m.Author.Username
            })
            .ToListAsync();

        return messages;
    }

    public async Task PostMessagesForUser(string username, string content)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Username == username);
        if (user is null)
        {
            throw new ArgumentException("User not found");
        }

        var newMessage = new Message
        {
            AuthorId = user.Id,
            Text = content,
            CreatedAt = DateTime.UtcNow,
            Flagged = false
        };

        await _dbContext.Messages.AddAsync(newMessage);
        await _dbContext.SaveChangesAsync();
    }
}