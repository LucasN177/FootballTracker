using System.Net.Http.Json;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TeamService
{
    private readonly HttpClient _httpClient = new();
    public List<Team>? Teams { get; set; }

    public List<Game>? Games { get; set; }
    
    public Game? LastGame { get; set; }
    public async Task<List<Team>> GetAllTeams(int year)
    {
        if(Teams != null)
            return Teams;
        var apiUrl = $"https://api.openligadb.de/getavailableteams/bl1/{year}";
        var apiTeams = await _httpClient.GetFromJsonAsync<List<ApiTeamResponse>>(apiUrl);

        if (apiTeams == null) 
            return new List<Team>();
        
        var teams = new List<Team>();

        foreach (var t in apiTeams)
        {
            teams.Add(new Team
            {
                Name = t.teamName,
                ShortName = t.shortName,
                LogoUrl = t.teamIconUrl,
                Id = t.teamInfoId
            });
        }
        Teams = teams;
        return teams;
    }

    public async Task<Game?> GetLastGame()
    {
        if(LastGame != null)
            return LastGame;
        var result =
            await _httpClient.GetFromJsonAsync<Game>("https://api.openligadb.de/getlastmatchbyleagueteam/4821/40");
        if (result != null)
            LastGame = result;
        return LastGame;
    }
    
    public async Task<List<Game>?> GetGames(int teamId, int weekCountPast, int weekCountFuture)
    {
        //if(Games is not null && Games.Count != 0)
            //return Games;
        var result =
            await _httpClient.GetFromJsonAsync<List<Game>>($"https://api.openligadb.de/getmatchesbyteamid/{teamId}/{weekCountPast}/{weekCountFuture}");
        if (result != null)
            Games = result;
        return Games;
    }


}