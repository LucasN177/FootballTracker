using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models.Api_Response_Models;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IOpenLigaDataRepository
{
    public Task<IResponse<ApiGameResponse>> GetMatchData();
    
    public Task<IResponse<List<ApiGameResponse>>> GetAllMatchesPerSeasonPerTeam(string leagueShortcut, string year, string teamShortName);
    
    public Task<IResponse<List<ApiGameResponse>>> GetMatchesBetweenTeams(string team1, string team2);

    public Task<IResponse<ApiGameResponse>> GetNextMatchByTeam(int leagueId, int teamId);
    
    public Task<IResponse<ApiGameResponse>> GetLastMatchByTeam(int leagueId, int teamId);
    
    public Task<IResponse<ApiGameResponse>> GetNextLeagueMatch(string leagueShortcut);
    
    public Task<IResponse<ApiGameResponse>> GetLastLeagueMatch(string leagueShortcut);

    public Task<IResponse<List<ApiGameResponse>>> GetCurrentGroup(string leagueShortcut);
    
    public Task<IResponse<List<ApiGoalGetterResponse>>> GetGoalGetters(string leagueShortcut, int year);
    
    public Task<IResponse<List<ApiTeamResponse>>> GetTeams(string leagueShortcut, int year);

    public Task<IResponse<List<ApiTeamResponse>>> GetTable(string leagueShortcut, int year);
    
    public Task<IResponse<List<ApiGameResponse>>> GetMatchesByTeam(int teamId, int weekCountPast, int weekCountFuture);
}