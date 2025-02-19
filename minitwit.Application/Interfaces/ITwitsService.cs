using minitwit.Domain.Entities;
using minitwit.Infrastructure.Dtos;

namespace minitwit.Application.Interfaces;

public interface ITwitsService
{
    public Task<IEnumerable<Message>> GetPublicTimeline(int page);
    public Task<IEnumerable<Message>> GetFeed(int userId, int page);
    public Task<IEnumerable<Message>> GetUsersTwits(int userId, int page);
    public Task<IEnumerable<Message>> GetUsersTwitsByName(string userName, int page);
    public Task<Message> PostTwit(int userId, string text);
    public Task<int> GetLatestProcessedCommandId();
    public Task<IEnumerable<MessageDto>> GetMessages(int limit);
    public Task<IEnumerable<MessageDto>> GetMessagesForUser(string username, int noMsgs);
    public Task PostMessagesForUser(string username, string content);
}