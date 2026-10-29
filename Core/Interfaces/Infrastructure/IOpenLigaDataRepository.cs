using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;

namespace FootballTracker.Core.Interfaces.Infrastructure;

public interface IOpenLigaDataRepository
{
    public Task<IResponse<List<Game>>> GetMatchData(string leagueShortcut, int year);

    public Task<IResponse<List<Game>>> GetMatchDay(string leagueShortcut, int year, int matchDay);

    public Task<IResponse<List<Game>>> GetAllMatchesPerSeasonPerTeam(string leagueShortcut, int year, string teamFilter);

    public Task<IResponse<List<Game>>> GetMatchesBetweenTeams(int teamId1, int teamId2);

    public Task<IResponse<Game>> GetNextMatchByTeam(int leagueId, int teamId);

    public Task<IResponse<Game>> GetLastMatchByTeam(int leagueId, int teamId);

    public Task<IResponse<Game>> GetNextLeagueMatch(string leagueShortcut);

    public Task<IResponse<Game>> GetLastLeagueMatch(string leagueShortcut);

    public Task<IResponse<Group>> GetCurrentGroup(string leagueShortcut);

    public Task<IResponse<List<ApiGoalGetterResponse>>> GetGoalGetters(string leagueShortcut, int year);

    public Task<IResponse<List<ApiTeamResponse>>> GetTeams(string leagueShortcut, int year);

    public Task<IResponse<List<ApiTeamResponse>>> GetTable(string leagueShortcut, int year);

    public Task<IResponse<List<Game>>> GetMatchesByTeam(int teamId, int weekCountPast, int weekCountFuture);
}
