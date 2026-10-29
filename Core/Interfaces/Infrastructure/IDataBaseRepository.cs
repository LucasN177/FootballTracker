using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Database;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IDataBaseRepository
{
    public Task<IResponse<List<FavoritePlayer>>> GetFavoritePlayers();

    public Task<IResponse<List<FavoriteTeam>>> GetFavoriteTeams();

    public Task<IResponse> AddPlayerToFavorites(Player player);

    public Task<IResponse> AddTeamToFavorites(Team team);

    public Task<IResponse> DeletePlayerFromFavorites(int playerId);

    public Task<IResponse> DeleteTeamFromFavorites(int teamId);
}
