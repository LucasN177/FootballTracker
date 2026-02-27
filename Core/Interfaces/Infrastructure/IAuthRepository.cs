using FootballTracker.Core.Interfaces.Response;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IAuthRepository
{
    public Task<IResponse> Login(string username, string password);
    
    public Task<IResponse> Register(string username, string password);
    
    public Task<IResponse> Logout();
}