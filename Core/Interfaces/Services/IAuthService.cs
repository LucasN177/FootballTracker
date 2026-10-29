using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models.Database;
using Supabase.Gotrue;

namespace FootballTracker.Core.Interfaces.Services;

public interface IAuthService
{
    public event Action? AuthStateChanged;

    public Session? CurrentSession { get; set; }
    public User? CurrentUser { get; set; }
    public bool IsLoggedIn { get; }

    /// <summary>Gibt an, ob Supabase-Zugangsdaten konfiguriert sind.</summary>
    public bool IsConfigured { get; }

    public Task<IResponse<User?>> GetCurrentUser();

    public Task<IResponse<Session?>> GetCurrentSession();

    public Task<IResponse<Session>> Login(string username, string password);

    public Task<IResponse<Session>> Register(string username, string password, UserDto? metadata = null);

    public Task<IResponse> Logout();

    public Task<IResponse> ResetPassword(string email);

    public Task<IResponse> InsertUserMetadata(UserDto user);

    public Task<IResponse> UpdateUserMetadata(UserDto user);

    public Task<IResponse<UserDto>> GetUserMetadata(string id);
}
