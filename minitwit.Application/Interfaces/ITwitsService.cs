using minitwit.Domain.Entities;
using minitwit.Infrastructure.Dtos;

namespace minitwit.Application.Interfaces;

public interface ITwitsService
{
    public Task<IEnumerable<Message>> GetPublicTimeline(int page);
    public Task<IEnumerable<Message>> GetFeed(int userId, int page);
    public Task<IEnumerable<Message>> GetUsersTwits(int userId, int page);
    public Task<Message> PostTwit(int userId, string text);

}