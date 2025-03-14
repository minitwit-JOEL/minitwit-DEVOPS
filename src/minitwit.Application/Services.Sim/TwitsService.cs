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
    private readonly ISimService _simService;

    public TwitsService(ApplicationDbContext dbContext, ISimService simService)
    {
        _dbContext = dbContext;
        _simService = simService;
    }
    
    public async Task<IEnumerable<MessageDto>> GetMessages(int latest, int limit = 100)
    {
        await _simService.UpdateLatest(latest);
        
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

    public async Task<IEnumerable<MessageDto>> GetMessagesForUser(int latest, string username, int no)
    {
        await _simService.UpdateLatest(latest);
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
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

    public async Task PostMessagesForUser(int latest, string username, string content)
    {
        await _simService.UpdateLatest(latest);
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user is not null)
        {

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
}