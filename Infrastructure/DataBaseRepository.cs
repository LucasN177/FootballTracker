using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;

namespace FootballTracker.Infrastructure;

public class DataBaseRepository(Supabase.Client client) : IDataBaseRepository
{
    public Task<IResponse<List<Player>>> GetFavoritePlayers()
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<List<Team>>> GetFavoriteTeams()
    {
        throw new NotImplementedException();
    }

    public async Task<IResponse> AddPlayerToFavorites(Player player)
    {
        var model = new FavoritePlayer()
        {
            PlayerId = player.Id,
            UserId = 1 //Todo: Aktueller User
        };
        
        var result = await client.From<FavoritePlayer>().Insert(model);
        if (result.Model != null)
            return Response.Success();
        return Response.Failure("Failed to insert favorite player", new Exception("Failed to insert favorite player"));
    }

    public Task<IResponse> AddTeamToFavorites(Team team)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse> DeletePlayerFromFavorites(Player player)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse> DeleteTeamFromFavorites(Team team)
    {
        throw new NotImplementedException();
    }
}