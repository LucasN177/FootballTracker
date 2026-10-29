using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Models;

namespace FootballTracker.Services;

public class TabellenService(IOpenLigaDataRepository openLigaDataRepository, StatisticsService statisticsService)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private readonly Dictionary<int, (DateTime LoadedAt, List<Team> Teams)> _tableCache = new();

    public async Task<List<Team>> GetBundesligaTabelle(int year, bool forceRefresh = false)
    {
        if (!forceRefresh && _tableCache.TryGetValue(year, out var cached)
                          && (year != AppStateService.CurrentSeasonYear || DateTime.Now - cached.LoadedAt < CacheDuration))
            return cached.Teams;

        var result = await openLigaDataRepository.GetTable(AppStateService.LeagueShortcut, year);
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
        _tableCache[year] = (DateTime.Now, teams);
        return teams;
    }

    public async Task<List<GoalGetter>> GetGoalGetters(string leagueShortcut, int year)
    {
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
