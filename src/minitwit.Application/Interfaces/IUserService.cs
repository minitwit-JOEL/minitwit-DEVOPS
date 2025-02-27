using minitwit.Domain.Entities;

namespace minitwit.Application.Interfaces;

public interface IUserService
{
    public Task<User?> GetLoggedInUser(int userId);
    public Task<User> GetUserByIdAsync(int userId);
    public Task<User?> GetUserByUsernameAsync(string username);
}