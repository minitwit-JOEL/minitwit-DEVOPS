using minitwit.Infrastructure.Dtos.Sim;

namespace minitwit.Application.Interfaces.Sim;

public interface IAuthService
{
    public Task<Result> Register(int latest, string username, string email, string password);
}