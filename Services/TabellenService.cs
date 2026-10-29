using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TabellenService(IOpenLigaDataRepository openLigaDataRepository, StatisticsService statisticsService, AppStateService appStateService, GameService gameService)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private readonly Dictionary<(string League, int Season), (DateTime LoadedAt, List<Team> Teams)> _tableCache = new();

    /// <summary>Tabelle des gewählten Wettbewerbs. Pokalwettbewerbe haben keine Tabelle (leere Liste).</summary>
    public async Task<List<Team>> GetTabelle(int year, bool forceRefresh = false)
    {
        if (appStateService.SelectedCompetition.IsCup)
            return new List<Team>();

        var key = (appStateService.League, year);
        if (!forceRefresh && _tableCache.TryGetValue(key, out var cached)
                          && (year != AppStateService.CurrentSeasonYear || DateTime.Now - cached.LoadedAt < CacheDuration))
            return cached.Teams;

        if (appStateService.SelectedCompetition.LeaguePhaseRounds > 0)
        {
            var leaguePhaseTable = await BuildLeaguePhaseTable(year);
            if (leaguePhaseTable.Any())
            {
                _tableCache[key] = (DateTime.Now, leaguePhaseTable);
                return leaguePhaseTable;
            }
        }

        var result = await openLigaDataRepository.GetTable(key.League, year);
        if (result is not { IsSuccess: true, Data: not null })
            throw new HttpRequestException(result.ErrorMessage ?? "Tabelle konnte nicht geladen werden.", result.Exception);

        var teams = new List<Team>();
        int platz = 1;

        foreach (var t in result.Data)
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

        await statisticsService.EnrichTeamsAsync(year, teams);
        _tableCache[key] = (DateTime.Now, teams);
        return teams;
    }

    /// <summary>
    /// Tabelle der Ligaphase (Champions League). OpenLigaDB rechnet dort auch K.-o.-Spiele ein,
    /// daher wird sie aus den Spielen der Ligaphase berechnet.
    /// </summary>
    private async Task<List<Team>> BuildLeaguePhaseTable(int year)
    {
        var competition = appStateService.SelectedCompetition;
        var games = (await gameService.GetSeasonGamesAsync(year)).Where(competition.IsLeaguePhase).ToList();
        var teams = StatisticsService.BuildRecords(games)
            .Select((record, index) => new Team
            {
                Id = record.Team.Id,
                Name = record.Team.Name,
                ShortName = record.Team.ShortName,
                LogoUrl = record.Team.LogoUrl,
                TabellenPlatz = index + 1,
                Spiele = record.Played,
                Siege = record.Won,
                Unentschieden = record.Draw,
                Niederlagen = record.Lost,
                Tore = record.GoalsFor,
                Gegentore = record.GoalsAgainst,
                Punkte = record.Points
            })
            .ToList();

        await statisticsService.EnrichTeamsAsync(year, teams);
        return teams;
    }

    /// <summary>
    /// Alle Teams der Saison: in Ligen die Tabelle, in Pokalwettbewerben die Teilnehmer (ohne Tabellenwerte).
    /// </summary>
    public async Task<List<Team>> GetSeasonTeams(int year)
    {
        if (appStateService.SelectedCompetition.IsLeague)
            return await GetTabelle(year);

        var result = await openLigaDataRepository.GetTeams(appStateService.League, year);
        if (result is not { IsSuccess: true, Data: not null })
            throw new HttpRequestException(result.ErrorMessage ?? "Teams konnten nicht geladen werden.", result.Exception);

        var teams = result.Data
            .Select(t => new Team { Id = t.teamInfoId, Name = t.teamName, ShortName = t.shortName, LogoUrl = t.teamIconUrl })
            .ToList();
        await statisticsService.EnrichTeamsAsync(year, teams);
        return teams;
    }

    public async Task<List<GoalGetter>> GetGoalGetters(string leagueShortcut, int year)
    {
        if (appStateService.SelectedCompetition.IsCup)
        {
            // Die Torjägerliste von OpenLigaDB kann im Pokal Tore aus dem Elfmeterschießen enthalten,
            // daher wird sie aus den Spieldaten berechnet.
            return (await statisticsService.GetPlayerStatsAsync(year))
                .Where(p => p.GoalCount > 0)
                .Select(p => new GoalGetter { Id = p.PlayerId, Name = p.Name, GoalCount = p.GoalCount, Platzierung = p.Rank, Team = p.Team })
                .ToList();
        }

        var result = await openLigaDataRepository.GetGoalGetters(leagueShortcut, year);
        if (result is not { IsSuccess: true, Data: not null })
            throw new HttpRequestException(result.ErrorMessage ?? "Torschützen konnten nicht geladen werden.", result.Exception);

        var playerStats = await statisticsService.GetPlayerStatsAsync(year);
        var teamsByPlayer = playerStats.ToDictionary(p => p.PlayerId, p => p.Team);

        var goalScorerList = new List<GoalGetter>();
        var rank = 0;
        var previousGoals = -1;
        foreach (var (scorer, index) in result.Data.OrderByDescending(s => s.GoalCount).Select((s, i) => (s, i)))
        {
            // Gleiche Toranzahl => gleicher Platz
            if (scorer.GoalCount != previousGoals)
                rank = index + 1;
            previousGoals = scorer.GoalCount;

            goalScorerList.Add(new GoalGetter
            {
                Id = scorer.GoalGetterId,
                Name = scorer.GoalGetterName.Trim(),
                GoalCount = scorer.GoalCount,
                Platzierung = rank,
                Team = teamsByPlayer.GetValueOrDefault(scorer.GoalGetterId)
            });
        }
        return goalScorerList;
    }
}
