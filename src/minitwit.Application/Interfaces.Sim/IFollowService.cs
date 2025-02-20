namespace minitwit.Application.Interfaces.Sim;

public interface IFollowService
{
    public Task<IEnumerable<string>> GetFollowerNames(int latest, string username, int no);
    public Task Follow(int latest, string username, string followerName);
    public Task Unfollow(int latest, string username, string followerName);
}