using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;

namespace FootballTracker.Infrastructure;

public class DataBaseRepository(Supabase.Client client) : IDataBaseRepository
{
    public async Task<IResponse<List<FavoritePlayer>>> GetFavoritePlayers()
    {
        if (CurrentUserId is not { } userId)
            return Response<List<FavoritePlayer>>.Failure("Not logged in");
        try
        {
            var result = await client.From<FavoritePlayer>().Where(x => x.UserId == userId).Get();
            return Response<List<FavoritePlayer>>.Success(result.Models);
        }
        catch (Exception ex)
        {
            return Response<List<FavoritePlayer>>.Failure(ex);
        }
    }

    public async Task<IResponse<List<FavoriteTeam>>> GetFavoriteTeams()
    {
        if (CurrentUserId is not { } userId)
            return Response<List<FavoriteTeam>>.Failure("Not logged in");
        try
        {
            var result = await client.From<FavoriteTeam>().Where(x => x.UserId == userId).Get();
            return Response<List<FavoriteTeam>>.Success(result.Models);
        }
        catch (Exception ex)
        {
            return Response<List<FavoriteTeam>>.Failure(ex);
        }
    }

    public async Task<IResponse> AddPlayerToFavorites(Player player)
    {
        if (CurrentUserId is not { } userId)
            return Response.Failure("Not logged in", new Exception("Not logged in"));
        var model = new FavoritePlayer
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            UserId = userId
        };

        try
        {
            var result = await client.From<FavoritePlayer>().Insert(model);
            return result.Model != null ? Response.Success() : Response.Failure("Failed to insert favorite player", new Exception("Failed to insert favorite player"));
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message, ex);
        }
    }

    public async Task<IResponse> AddTeamToFavorites(Team team)
    {
        if (CurrentUserId is not { } userId)
            return Response.Failure("Not logged in", new Exception("Not logged in"));
        var model = new FavoriteTeam
        {
            TeamId = team.Id,
            TeamName = team.Name,
            TeamIconUrl = team.LogoUrl,
            UserId = userId
        };

        try
        {
            var result = await client.From<FavoriteTeam>().Insert(model);
            return result.Model != null ? Response.Success() : Response.Failure("Failed to insert favorite team", new Exception("Failed to insert favorite team"));
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message, ex);
        }
    }

    public async Task<IResponse> DeletePlayerFromFavorites(int playerId)
    {
        if (CurrentUserId is not { } userId)
            return Response.Failure("Not logged in", new Exception("Not logged in"));
        try
        {
            await client.From<FavoritePlayer>()
                .Where(x => x.UserId == userId && x.PlayerId == playerId)
                .Delete();
            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message, ex);
        }
    }

    public async Task<IResponse> DeleteTeamFromFavorites(int teamId)
    {
        if (CurrentUserId is not { } userId)
            return Response.Failure("Not logged in", new Exception("Not logged in"));
        try
        {
            await client.From<FavoriteTeam>()
                .Where(x => x.UserId == userId && x.TeamId == teamId)
                .Delete();
            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message, ex);
        }
    }

    private string? CurrentUserId => client.Auth.CurrentUser?.Id;
}
