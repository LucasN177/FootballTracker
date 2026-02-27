using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IDataBaseRepository
{
    public Task<IResponse<List<Player>>> GetFavoritePlayers();
    
    public Task<IResponse<List<Team>>> GetFavoriteTeams();
    
    public Task<IResponse> AddPlayerToFavorites(Player player);
    
    public Task<IResponse> AddTeamToFavorites(Team team);
    
    public Task<IResponse> DeletePlayerFromFavorites(Player player);
    
    public Task<IResponse> DeleteTeamFromFavorites(Team team);
}