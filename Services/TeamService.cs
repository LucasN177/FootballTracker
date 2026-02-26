using System.Net.Http.Json;
using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;
using FootballTracker.Pages.Components;

namespace FootballTracker.Services;

public class TeamService(IOpenLigaDataRepository openLigaDataRepository)
{
    private readonly HttpClient _httpClient = new();
    public List<Team>? Teams { get; set; }

    public List<Game>? Games { get; set; }
    
    public Game? LastGame { get; set; }
    public async Task<List<Team>> GetAllTeams(string leagueShortcut, int year)
    {
        var result = await openLigaDataRepository.GetTeams(leagueShortcut, year);
        
        var teams = new List<Team>();
        if (result is { IsSuccess: true, Data: not null })
        {
            foreach (var t in result.Data)
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
        }
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
        var result = await openLigaDataRepository.GetMatchesByTeam(teamId, weekCountPast, weekCountFuture);

        if (result is { IsSuccess: true, Data: not null })
        {
            var gamesList = new List<Game>();
            foreach (var game in result.Data)
            {
                gamesList.Add(new Game()
                {
                    MatchId =  game.MatchId,
                    MatchDateTime =  game.MatchDateTime,
                    TimeZoneId =  game.TimeZoneId,
                    LeagueId = game.LeagueId,
                    LeagueName = game.LeagueName,
                    LeagueSeason = game.LeagueSeason,
                    LeagueShortcut = game.LeagueShortcut,
                    MatchDateTimeUtc = game.MatchDateTimeUtc,
                    Team1 = game.Team1,
                    Team2 = game.Team2,
                    LastUpdateDateTime = game.LastUpdateDateTime,
                    MatchIsFinished = game.MatchIsFinished,
                    MatchResults = game.MatchResults,
                    Goals = game.Goals,
                    Location = game.Location,
                    NumberOfViewers =  game.NumberOfViewers
                });
            }
            Games = gamesList;
        }
        return Games;
    }


}