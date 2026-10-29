using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;
using Supabase.Gotrue;

namespace FootballTracker.Services;

public class AuthService(IAuthRepository authRepository, SupabaseConfig config) : IAuthService
{
    public event Action? AuthStateChanged;

    public Session? CurrentSession { get; set; }
    public User? CurrentUser { get; set; }
    public bool IsLoggedIn => CurrentSession?.AccessToken != null;
    public bool IsConfigured => config.IsConfigured;

    public async Task<IResponse<User?>> GetCurrentUser()
    {
        var result =  await authRepository.GetCurrentUser();
        if (result.IsSuccess)
        {
            CurrentUser = result.Data;
            return Response<User?>.Success(result.Data);
        }
        return Response<User?>.Failure(result.ErrorMessage ?? "No current user");
    }

    public async Task<IResponse<Session?>> GetCurrentSession()
    {
        var result =  await authRepository.GetCurrentSession();
        if (result.IsSuccess)
        {
            CurrentSession = result.Data;
            CurrentUser = result.Data?.User;
            return Response<Session?>.Success(result.Data);
        }
        return Response<Session?>.Failure(result.ErrorMessage ?? "No current session");
    }

    public async Task<IResponse<Session>> Login(string username, string password)
    {
        if (!IsConfigured)
            return Response<Session>.Failure("Supabase ist nicht konfiguriert.");
        var result =  await authRepository.Login(username, password);
        if (result is not { IsSuccess: true, Data: not null })
            return Response<Session>.Failure(result.ErrorMessage ?? "Login failed");

        SetSession(result.Data);
        return Response<Session>.Success(result.Data);
    }

    public async Task<IResponse<Session>> Register(string username, string password, UserDto? metadata = null)
    {
        if (!IsConfigured)
            return Response<Session>.Failure("Supabase ist nicht konfiguriert.");
        var result =  await authRepository.Register(username, password, metadata);
        if (result is not { IsSuccess: true, Data: not null })
            return Response<Session>.Failure(result.ErrorMessage ?? "Register failed.");

        // Ohne aktivierte E-Mail-Bestätigung liefert Supabase direkt eine gültige Session.
        if (result.Data.AccessToken != null)
            SetSession(result.Data);
        return Response<Session>.Success(result.Data);
    }

    public async Task<IResponse> Logout()
    {
        var result = await authRepository.Logout();
        SetSession(null);
        return result;
    }

    public Task<IResponse> ResetPassword(string email) => authRepository.ResetPassword(email);

    public Task<IResponse> InsertUserMetadata(UserDto user) => authRepository.InsertUserMetadata(user);

    public Task<IResponse> UpdateUserMetadata(UserDto user) => authRepository.UpdateUserMetadata(user);

    public async Task<IResponse<UserDto>> GetUserMetadata(string id)
    {
        var result = await authRepository.GetUserMetadata(id);
        return result is { IsSuccess: true, Data: not null } ? Response<UserDto>.Success(result.Data) : Response<UserDto>.Failure(result.ErrorMessage ?? "No metadata found");
    }

    private void SetSession(Session? session)
    {
        CurrentSession = session;
        CurrentUser = session?.User;
        AuthStateChanged?.Invoke();
    }
}
