using System.Net;
using System.Net.Http.Json;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;

namespace FootballTracker.Infrastructure;

public class OpenLigaDataRepository(HttpClient httpClient) : IOpenLigaDataRepository
{
    private const string BaseUrl = "https://api.openligadb.de/";

    public Task<IResponse<List<Game>>> GetMatchData(string leagueShortcut, int year)
        => Get<List<Game>>($"getmatchdata/{leagueShortcut}/{year}");

    public Task<IResponse<List<Game>>> GetMatchDay(string leagueShortcut, int year, int matchDay)
        => Get<List<Game>>($"getmatchdata/{leagueShortcut}/{year}/{matchDay}");

    public Task<IResponse<List<Game>>> GetAllMatchesPerSeasonPerTeam(string leagueShortcut, int year, string teamFilter)
        => Get<List<Game>>($"getmatchdata/{leagueShortcut}/{year}/{Uri.EscapeDataString(teamFilter)}");

    public Task<IResponse<List<Game>>> GetMatchesBetweenTeams(int teamId1, int teamId2)
        => Get<List<Game>>($"getmatchdata/{teamId1}/{teamId2}");

    public Task<IResponse<Game>> GetNextMatchByTeam(int leagueId, int teamId)
        => Get<Game>($"getnextmatchbyleagueteam/{leagueId}/{teamId}");

    public Task<IResponse<Game>> GetLastMatchByTeam(int leagueId, int teamId)
        => Get<Game>($"getlastmatchbyleagueteam/{leagueId}/{teamId}");

    public Task<IResponse<Game>> GetNextLeagueMatch(string leagueShortcut)
        => Get<Game>($"getnextmatchbyleagueshortcut/{leagueShortcut}");

    public Task<IResponse<Game>> GetLastLeagueMatch(string leagueShortcut)
        => Get<Game>($"getlastmatchbyleagueshortcut/{leagueShortcut}");

    public Task<IResponse<Group>> GetCurrentGroup(string leagueShortcut)
        => Get<Group>($"getcurrentgroup/{leagueShortcut}");

    public Task<IResponse<List<ApiGoalGetterResponse>>> GetGoalGetters(string leagueShortcut, int year)
        => Get<List<ApiGoalGetterResponse>>($"getgoalgetters/{leagueShortcut}/{year}");

    public Task<IResponse<List<ApiTeamResponse>>> GetTeams(string leagueShortcut, int year)
        => Get<List<ApiTeamResponse>>($"getavailableteams/{leagueShortcut}/{year}");

    public Task<IResponse<List<ApiTeamResponse>>> GetTable(string leagueShortcut, int year)
        => Get<List<ApiTeamResponse>>($"getbltable/{leagueShortcut}/{year}");

    public Task<IResponse<List<Game>>> GetMatchesByTeam(int teamId, int weekCountPast, int weekCountFuture)
        => Get<List<Game>>($"getmatchesbyteamid/{teamId}/{weekCountPast}/{weekCountFuture}");

    private async Task<IResponse<T>> Get<T>(string path)
    {
        try
        {
            using var response = await httpClient.GetAsync(BaseUrl + path);
            if (response.StatusCode == HttpStatusCode.NoContent)
                return Response<T>.Failure("Keine Daten verfügbar");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<T>();
            return result == null
                ? Response<T>.Failure("Keine Daten verfügbar")
                : Response<T>.Success(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenLigaDB request '{path}' failed: {ex.Message}");
            return Response<T>.Failure(ex);
        }
    }
}
