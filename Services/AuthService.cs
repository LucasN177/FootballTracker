using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Interfaces.Services;
using FootballTracker.Core.Models;
using Supabase.Gotrue;

namespace FootballTracker.Services;

public class AuthService(IAuthRepository authRepository) : IAuthService
{
    public Session? CurrentSession { get; set; }
    public User? CurrentUser { get; set; } 
    
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
            return Response<Session?>.Success(result.Data);
        } 
        return Response<Session?>.Failure(result.ErrorMessage ?? "No current session");
    }

    public async Task<IResponse<Session>> Login(string username, string password)
    {
        var result =  await authRepository.Login(username, password);
        return result is { IsSuccess: true, Data: not null } ? Response<Session>.Success(result.Data) : Response<Session>.Failure(result.ErrorMessage ?? "Login failed");
    }

    public async Task<IResponse<Session>> Register(string username, string password)
    {
        var result =  await authRepository.Register(username, password);
        return result is { IsSuccess: true, Data: not null } ? Response<Session>.Success(result.Data) : Response<Session>.Failure(result.ErrorMessage ?? "Register failed.");
    }

    public Task<IResponse> Logout()
    {
        CurrentSession = null;
        CurrentUser = null;
        authRepository.Logout();
        return Task.FromResult(Response.Success());
    }
}