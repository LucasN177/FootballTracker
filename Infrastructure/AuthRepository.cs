using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using Supabase.Gotrue;

namespace FootballTracker.Infrastructure;

public class AuthRepository(Supabase.Client client) :IAuthRepository
{
    public async Task<IResponse<Session>> Login(string username, string password)
    {
        var result = await client.Auth.SignInWithPassword(username, password);
        return result is not null ? Response<Session>.Success(result) : Response<Session>.Failure("Login failed");
    }

    public async Task<IResponse<Session>> Register(string username, string password)
    {
        var result = await client.Auth.SignUp(username, password);
        return result is not null ? Response<Session>.Success(result) : Response<Session>.Failure("Registration failed");
    }

    public async Task<IResponse> Logout()
    {
        await client.Auth.SignOut();
        return Response.Success();
    }

    public Task<IResponse<Session>> GetCurrentSession()
    {
        try
        {
            return Task.FromResult(client.Auth.CurrentSession is not null ? Response<Session>.Success(client.Auth.CurrentSession) : Response<Session>.Failure("No login available"));
        }
        catch (Exception exception)
        {
            return Task.FromException<IResponse<Session>>(exception);
        }
    }

    public Task<IResponse<User>> GetCurrentUser()
    {
        try
        {
            return Task.FromResult(client.Auth.CurrentUser is not null ? Response<User>.Success(client.Auth.CurrentUser) : Response<User>.Failure("No login available"));
        }
        catch (Exception exception)
        {
            return Task.FromException<IResponse<User>>(exception);
        }
    }

    public Task<IResponse> InsertUserMetadata()
    {
        throw new NotImplementedException();
    }

    public Task<IResponse> UpdateUserMetadata()
    {
        throw new NotImplementedException();
    }

    public Task<IResponse> GetUserMetadata()
    {
        throw new NotImplementedException();
    }
}