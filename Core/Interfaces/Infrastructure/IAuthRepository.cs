using FootballTracker.Core.Interfaces.Response;
using Supabase.Gotrue;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IAuthRepository
{
    public Task<IResponse<Session>> Login(string username, string password);
    
    public Task<IResponse<Session>> Register(string username, string password);
    
    public Task<IResponse> Logout();
    
    public Task<IResponse<Session>> GetCurrentSession();
    
    public Task<IResponse<User>> GetCurrentUser();
}