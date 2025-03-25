using minitwit.Infrastructure.Dtos;
using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Interfaces.Sim;

public interface ITwitsService
{
    public Task<IEnumerable<MessageDto>> GetMessages(int latest, int limit);
    public Task<IEnumerable<MessageDto>> GetPrivateTimeline(int latest, string username, int no);
    public Task<IEnumerable<MessageDto>> GetMessagesForUser(int latest, string username, int no);
    public Task PostMessagesForUser(int latest, string username, string content);
}