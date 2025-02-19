using minitwit.Infrastructure.Dtos;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Interfaces.Sim;

public interface ITwitsService
{
    public Task<IEnumerable<MessageDto>> GetMessages(int limit);
    public Task<IEnumerable<MessageDto>> GetMessagesForUser(string username, int noMsgs);
    public Task PostMessagesForUser(string username, string content);
}