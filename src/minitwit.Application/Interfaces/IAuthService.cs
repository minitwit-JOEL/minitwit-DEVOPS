using minitwit.Domain.Entities;

namespace minitwit.Application.Interfaces;

public interface IAuthService
{
    public Task<User?> Login(string username, string password);
    public Task<User?> Register(string username, string email, string password, string confirmPassword);
}