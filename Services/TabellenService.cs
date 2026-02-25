using System.Net.Http.Json;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TabellenService
{
    private readonly HttpClient _httpClient = new();
    public async Task<List<Team>> GetBundesligaTabelle(int year)
    {
        // API-Aufruf
        var apiUrl = $"https://api.openligadb.de/getbltable/bl1/{year}";

        // Das JSON-Array von der API holen
        var apiTeams = await _httpClient.GetFromJsonAsync<List<ApiTeamResponse>>(apiUrl);

        if (apiTeams == null) 
            return new List<Team>();

        // Mapping in dein Team-Modell
        var teams = new List<Team>();
        int platz = 1;

        foreach (var t in apiTeams)
        {
            teams.Add(new Team
            {
                TabellenPlatz = platz++,
                Name = t.teamName,
                ShortName = t.shortName,
                LogoUrl = t.teamIconUrl,
                Spiele = t.matches,
                Siege = t.won,
                Unentschieden = t.draw,
                Niederlagen = t.lost,
                Tore = t.goals,
                Gegentore = t.opponentGoals,
                Punkte = t.points,
                Id = t.teamInfoId
                
            });
        }
        return teams;
    }
    
}