using System.Net.Http.Json;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;

namespace FootballTracker.Services;

public class TabellenService(IOpenLigaDataRepository openLigaDataRepository)
{
    private readonly HttpClient _httpClient = new();
    public async Task<List<Team>> GetBundesligaTabelle(int year)
    {
        var apiUrl = $"https://api.openligadb.de/getbltable/bl1/{year}";
        
        var apiTeams = await _httpClient.GetFromJsonAsync<List<ApiTeamResponse>>(apiUrl);

        if (apiTeams == null) 
            return new List<Team>();
        
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

    public async Task<List<GoalGetter>> GetGoalGetters(string leagueShortcut, int year)
    {
        var goalScorerList = new List<GoalGetter>();
        var result = await openLigaDataRepository.GetGoalGetters(leagueShortcut, year);
        var number = 1;
        if (result is { IsSuccess: true, Data: not null })
        {
            goalScorerList.AddRange(result.Data.Select(scorer => new GoalGetter() { Id = scorer.GoalGetterId, Name = scorer.GoalGetterName, GoalCount = scorer.GoalCount, Platzierung = number++}));
        }
        return goalScorerList;
    }
    
}