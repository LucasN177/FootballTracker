using System.Net.Http.Json;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TeamService
{
    private readonly HttpClient _httpClient = new();
    public async Task<List<Team>> GetAllTeams(int year)
    {
        var apiUrl = $"https://api.openligadb.de/getavailableteams/bl1/{year}";
        var apiTeams = await _httpClient.GetFromJsonAsync<List<ApiTeamResponse>>(apiUrl);

        if (apiTeams == null) 
            return new List<Team>();

        // Mapping in dein Team-Modell
        var teams = new List<Team>();

        foreach (var t in apiTeams)
        {
            teams.Add(new Team
            {
                Name = t.teamName,
                ShortName = t.shortName,
                LogoUrl = t.teamIconUrl,
            });
        }
        return teams;
    }
}