using System.Net.Http.Json;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;

namespace FootballTracker.Infrastructure;

public class OpenLigaDataRepository(HttpClient httpClient) : IOpenLigaDataRepository
{
    private const string BaseUrl = "https://api.openligadb.de/";
    
    private List<ApiGoalGetterResponse>? _cachedGoalGetters;
    private int? _calledYear;
    
    public Task<IResponse<ApiGameResponse>> GetMatchData()
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<List<ApiGameResponse>>> GetAllMatchesPerSeasonPerTeam(string leagueShortcut, string year, string teamShortName)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<List<ApiGameResponse>>> GetMatchesBetweenTeams(string team1, string team2)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<ApiGameResponse>> GetNextMatchByTeam(int leagueId, int teamId)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<ApiGameResponse>> GetLastMatchByTeam(int leagueId, int teamId)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<ApiGameResponse>> GetNextLeagueMatch(string leagueShortcut)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<ApiGameResponse>> GetLastLeagueMatch(string leagueShortcut)
    {
        throw new NotImplementedException();
    }

    public Task<IResponse<List<ApiGameResponse>>> GetCurrentGroup(string leagueShortcut)
    {
        throw new NotImplementedException();
    }

    public async Task<IResponse<List<ApiGoalGetterResponse>>> GetGoalGetters(string leagueShortcut, int year)
    {
        if (_cachedGoalGetters != null && _calledYear == year)
            return Response<List<ApiGoalGetterResponse>>.Success(_cachedGoalGetters);
        var result = await httpClient.GetFromJsonAsync<List<ApiGoalGetterResponse>>($"{BaseUrl}getgoalgetters/{leagueShortcut}/{year}");
        if (result == null)
            return Response<List<ApiGoalGetterResponse>>.Failure("Error");
        _cachedGoalGetters = result;
        _calledYear = year;
        return Response<List<ApiGoalGetterResponse>>.Success(result);
    }

    public async Task<IResponse<List<ApiTeamResponse>>> GetTeams(string leagueShortcut, int year)
    {
        var apiUrl = $"{BaseUrl}getavailableteams/{leagueShortcut}/{year}";
        var apiTeams = await httpClient.GetFromJsonAsync<List<ApiTeamResponse>>(apiUrl);

        if (apiTeams == null)
            return Response<List<ApiTeamResponse>>.Failure("Error");
        
        return Response<List<ApiTeamResponse>>.Success(apiTeams);
    }

    public Task<IResponse<List<ApiTeamResponse>>> GetTable(string leagueShortcut, int year)
    {
        throw new NotImplementedException();
    }

    public async Task<IResponse<List<ApiGameResponse>>> GetMatchesByTeam(int teamId, int weekCountPast, int weekCountFuture)
    {
        try
        {
            var result =
                await httpClient.GetFromJsonAsync<List<ApiGameResponse>>($"{BaseUrl}getmatchesbyteamid/{teamId}/{weekCountPast}/{weekCountFuture}");

            if (result == null)
                return Response<List<ApiGameResponse>>.Failure("Error");
            
            return Response<List<ApiGameResponse>>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        
    }
}