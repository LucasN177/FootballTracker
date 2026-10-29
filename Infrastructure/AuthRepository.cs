using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;

namespace FootballTracker.Infrastructure;

public class AuthRepository(Supabase.Client client) :IAuthRepository
{
    public async Task<IResponse<Session>> Login(string username, string password)
    {
        try
        {
            var result = await client.Auth.SignInWithPassword(username, password);
            return result is not null ? Response<Session>.Success(result) : Response<Session>.Failure("Login fehlgeschlagen");
        }
        catch (Exception ex)
        {
            return Response<Session>.Failure(GetErrorMessage(ex, "Login fehlgeschlagen"));
        }
    }

    public async Task<IResponse<Session>> Register(string username, string password, UserDto? metadata = null)
    {
        try
        {
            var options = new SignUpOptions();
            if (metadata != null)
            {
                options.Data = new Dictionary<string, object>
                {
                    ["username"] = metadata.Username,
                    ["vorname"] = metadata.Vorname,
                    ["nachname"] = metadata.Nachname
                };
            }

            var result = await client.Auth.SignUp(username, password, options);
            return result is not null ? Response<Session>.Success(result) : Response<Session>.Failure("Registrierung fehlgeschlagen");
        }
        catch (Exception ex)
        {
            return Response<Session>.Failure(GetErrorMessage(ex, "Registrierung fehlgeschlagen"));
        }
    }

    public async Task<IResponse> Logout()
    {
        try
        {
            await client.Auth.SignOut();
            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(GetErrorMessage(ex, "Logout fehlgeschlagen"), ex);
        }
    }

    public async Task<IResponse> ResetPassword(string email)
    {
        try
        {
            var result = await client.Auth.ResetPasswordForEmail(email);
            return result ? Response.Success() : Response.Failure("E-Mail konnte nicht gesendet werden", null);
        }
        catch (Exception ex)
        {
            return Response.Failure(GetErrorMessage(ex, "E-Mail konnte nicht gesendet werden"), ex);
        }
    }

    public Task<IResponse<Session>> GetCurrentSession()
    {
        return Task.FromResult(client.Auth.CurrentSession is not null ? Response<Session>.Success(client.Auth.CurrentSession) : Response<Session>.Failure("No login available"));
    }

    public Task<IResponse<User>> GetCurrentUser()
    {
        return Task.FromResult(client.Auth.CurrentUser is not null ? Response<User>.Success(client.Auth.CurrentUser) : Response<User>.Failure("No login available"));
    }

    public Task<IResponse> InsertUserMetadata(UserDto userDto) => UpsertUserMetadata(userDto);

    public Task<IResponse> UpdateUserMetadata(UserDto userDto) => UpsertUserMetadata(userDto);

    public async Task<IResponse<UserDto>> GetUserMetadata(string id)
    {
        try
        {
            var result = await client.From<UserDto>().Where(x => x.UserId == id).Get();
            return result.Model is not null ? Response<UserDto>.Success(result.Model) : Response<UserDto>.Failure("No user found");
        }
        catch (Exception ex)
        {
            return Response<UserDto>.Failure(ex);
        }
    }

    private async Task<IResponse> UpsertUserMetadata(UserDto userDto)
    {
        if (client.Auth.CurrentUser?.Id is not { } userId)
            return Response.Failure("Not logged in", null);
        try
        {
            userDto.UserId = userId;
            await client.From<UserDto>().Upsert(userDto);
            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message, ex);
        }
    }

    private static string GetErrorMessage(Exception ex, string fallback)
    {
        if (ex is not GotrueException gotrueException)
            return $"{fallback}: {ex.Message}";

        return gotrueException.Reason switch
        {
            FailureHint.Reason.UserEmailNotConfirmed => "Bitte bestätige zuerst deine E-Mail-Adresse.",
            FailureHint.Reason.UserBadLogin or FailureHint.Reason.UserBadPassword => "E-Mail oder Passwort ist falsch.",
            FailureHint.Reason.UserAlreadyRegistered => "Für diese E-Mail existiert bereits ein Konto.",
            FailureHint.Reason.UserBadEmailAddress => "Die E-Mail-Adresse ist ungültig.",
            FailureHint.Reason.UserTooManyRequests => "Zu viele Versuche – bitte warte kurz.",
            _ => fallback
        };
    }
}
