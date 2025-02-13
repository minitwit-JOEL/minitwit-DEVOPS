using minitwit.Domain.Entities;

namespace minitwit.Application.Interfaces;

public interface IFollowService
{
    public Task<Follow> AddFollow(int whoId, string whomUsername);
    public Task<Follow> RemoveFollow(int whoId, string whomUsername);
}