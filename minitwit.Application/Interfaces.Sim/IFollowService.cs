namespace minitwit.Application.Interfaces.Sim;

public interface IFollowService
{
    public Task<IEnumerable<string>> GetFollowerNames(string username, int no);
    public Task Follow(string username, string followerName);
    public Task Unfollow(string username, string followerName);
}