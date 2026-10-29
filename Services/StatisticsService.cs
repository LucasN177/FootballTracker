using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Extensions;
using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Services;

/// <summary>
/// Leitet Spieler- und Teamstatistiken aus den Spieldaten einer Saison ab.
/// OpenLigaDB liefert keine Kaderdaten, daher wird die Teamzugehörigkeit eines Spielers über seine Tore ermittelt.
/// </summary>
public class StatisticsService(GameService gameService, AppStateService appStateService)
{
    private readonly Dictionary<(string League, int Season), (int Version, List<PlayerSeasonStats> Stats)> _playerCache = new();

    public async Task<List<PlayerSeasonStats>> GetPlayerStatsAsync(int season)
    {
        var games = await gameService.GetSeasonGamesAsync(season);
        var key = (appStateService.League, season);
        if (_playerCache.TryGetValue(key, out var cached) && cached.Version == gameService.Version)
            return cached.Stats;

        var stats = BuildPlayerStats(games);
        _playerCache[key] = (gameService.Version, stats);
        return stats;
    }

    public async Task<PlayerSeasonStats?> GetPlayerAsync(int season, int playerId)
    {
        var stats = await GetPlayerStatsAsync(season);
        return stats.FirstOrDefault(p => p.PlayerId == playerId);
    }

    public static List<GoalEvent> GetGoalEvents(IEnumerable<Game> games)
    {
        var events = new List<GoalEvent>();
        foreach (var game in games)
        {
            int previous1 = 0, previous2 = 0;
            foreach (var goal in game.RegularGoals)
            {
                Team? scoringTeam = null;
                if (goal.ScoreTeam1 > previous1)
                    scoringTeam = game.Team1;
                else if (goal.ScoreTeam2 > previous2)
                    scoringTeam = game.Team2;

                previous1 = goal.ScoreTeam1;
                previous2 = goal.ScoreTeam2;

                // Inkonsistente Datensätze (Spielstand unverändert) werden übersprungen.
                if (scoringTeam == null)
                    continue;

                var playerTeam = goal.IsOwnGoal ? game.GetOpponent(scoringTeam.Id) : scoringTeam;
                events.Add(new GoalEvent { Game = game, Goal = goal, ScoringTeam = scoringTeam, PlayerTeam = playerTeam });
            }
        }
        return events;
    }

    private static List<PlayerSeasonStats> BuildPlayerStats(IEnumerable<Game> games)
    {
        var players = new Dictionary<int, PlayerSeasonStats>();
        foreach (var goalEvent in GetGoalEvents(games).Where(e => e.Goal.GoalGetterId > 0))
        {
            if (!players.TryGetValue(goalEvent.Goal.GoalGetterId, out var player))
            {
                player = new PlayerSeasonStats { PlayerId = goalEvent.Goal.GoalGetterId, Name = goalEvent.Goal.GoalGetterName.Trim() };
                players.Add(player.PlayerId, player);
            }
            player.Events.Add(goalEvent);
        }

        foreach (var player in players.Values)
        {
            // Bei Vereinswechseln zählt das zuletzt gespielte Team.
            player.Team = player.Events
                .OrderByDescending(e => e.Game.MatchDateTimeUtc)
                .Select(e => e.PlayerTeam)
                .FirstOrDefault();
        }

        var ranked = players.Values
            .OrderByDescending(p => p.GoalCount)
            .ThenBy(p => p.Name)
            .ToList();

        var rank = 0;
        var previousGoals = -1;
        for (var i = 0; i < ranked.Count; i++)
        {
            if (ranked[i].GoalCount != previousGoals)
                rank = i + 1;
            previousGoals = ranked[i].GoalCount;
            ranked[i].Rank = rank;
        }
        return ranked;
    }

    /// <summary>Ergänzt Tabelleneinträge um Form, letzte Spiele und das nächste Spiel.</summary>
    public async Task EnrichTeamsAsync(int season, List<Team> teams, int formLength = 5)
    {
        List<Game> games;
        try
        {
            games = await gameService.GetSeasonGamesAsync(season);
        }
        catch (HttpRequestException)
        {
            return;
        }

        foreach (var team in teams)
        {
            var teamGames = games.Where(g => g.Involves(team.Id)).OrderBy(g => g.MatchDateTimeUtc).ToList();
            team.LastGames = teamGames.Where(g => g.MatchIsFinished).TakeLast(formLength).ToList();
            team.Form = new string(team.LastGames.Select(g => g.GetOutcomeFor(team.Id) ?? '-').ToArray());
            team.NextGame = teamGames.FirstOrDefault(g => !g.MatchIsFinished);
        }
    }

    public static List<TeamRecord> BuildRecords(IEnumerable<Game> games, Func<Game, Team, bool>? filter = null)
    {
        var records = new Dictionary<int, TeamRecord>();
        foreach (var game in games.Where(g => g.MatchIsFinished && g.FinalResult != null))
        {
            foreach (var team in new[] { game.Team1, game.Team2 })
            {
                if (!records.TryGetValue(team.Id, out var record))
                {
                    record = new TeamRecord { Team = team };
                    records.Add(team.Id, record);
                }

                if (filter != null && !filter(game, team))
                    continue;

                var (own, other) = game.GetGoalsFor(team.Id);
                record.Played++;
                record.GoalsFor += own;
                record.GoalsAgainst += other;
                if (other == 0)
                    record.CleanSheets++;
                switch (game.GetOutcomeFor(team.Id))
                {
                    case 'S': record.Won++; break;
                    case 'U': record.Draw++; break;
                    case 'N': record.Lost++; break;
                }
            }
        }

        return records.Values
            .OrderByDescending(r => r.Points)
            .ThenByDescending(r => r.GoalDifference)
            .ThenByDescending(r => r.GoalsFor)
            .ToList();
    }

    /// <summary>Kumulierte Punkte eines Teams je Spieltag.</summary>
    public static double[] GetPointsProgression(IEnumerable<Game> games, int teamId, int matchDays)
    {
        var pointsPerDay = new double[matchDays];
        foreach (var game in games.Where(g => g.MatchIsFinished && g.Involves(teamId) && g.MatchDay >= 1 && g.MatchDay <= matchDays))
            pointsPerDay[game.MatchDay - 1] += game.GetPointsFor(teamId);

        var sum = 0d;
        return pointsPerDay.Select(p => sum += p).ToArray();
    }

    /// <summary>
    /// Tabellenplatz aller Teams nach jedem Spieltag (Index 0 = 1. Spieltag).
    /// Nachholspiele zählen zu ihrem ursprünglichen Spieltag.
    /// </summary>
    public static Dictionary<int, int[]> GetPositionHistory(IReadOnlyCollection<Game> games, int matchDays)
    {
        var history = new Dictionary<int, int[]>();
        foreach (var team in games.SelectMany(g => new[] { g.Team1, g.Team2 }).DistinctBy(t => t.Id))
            history[team.Id] = new int[matchDays];

        for (var day = 1; day <= matchDays; day++)
        {
            var records = BuildRecords(games.Where(g => g.MatchDay <= day));
            for (var i = 0; i < records.Count; i++)
                history[records[i].Team.Id][day - 1] = i + 1;
        }
        return history;
    }

    /// <summary>Tore pro Viertelstunde (0-15, 16-30, ..., 76-90+).</summary>
    public static double[] GetGoalsByMinute(IEnumerable<GoalEvent> events)
    {
        var buckets = new double[6];
        foreach (var minute in events.Where(e => e.Goal.MatchMinute.HasValue).Select(e => e.Goal.MatchMinute!.Value))
            buckets[Math.Clamp((minute - 1) / 15, 0, 5)]++;
        return buckets;
    }

    public static readonly string[] MinuteBucketLabels = ["1-15", "16-30", "31-45", "46-60", "61-75", "76-90+"];
}
