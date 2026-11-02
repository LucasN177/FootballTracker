using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;
using FootballTracker.Core.Models.Extensions;

namespace FootballTracker.Services;

/// <summary>Prognose eines Spieltags mit dem Modell, das auf allen vorher beendeten Spielen trainiert wurde.</summary>
public class MatchDayForecast
{
    public int MatchDay { get; init; }
    public PredictionModel? Model { get; init; }
    public List<MatchPrediction> Predictions { get; init; } = new();
}

/// <summary>Alles, was zur Aufschlüsselung einer einzelnen Prognose gebraucht wird.</summary>
public class MatchExplanation
{
    public required MatchPrediction Prediction { get; init; }
    public required PredictionModel Model { get; init; }

    /// <summary>Stand der Daten: Anstoß des ersten Spiels des Spieltags.</summary>
    public DateTime Cutoff { get; init; }

    /// <summary>Spiele, aus denen die Werte des Heim- bzw. Gastteams geschätzt wurden, neueste zuerst.</summary>
    public required List<(Game Game, double Weight)> HomeGames { get; init; }
    public required List<(Game Game, double Weight)> AwayGames { get; init; }
}

/// <summary>
/// Berechnet Spielprognosen mit dem <see cref="PredictionModel"/>. Trainiert wird mit der laufenden Saison und der Vorsaison
/// desselben Wettbewerbs, jeweils nur mit Spielen, die vor dem ersten Anstoß des Spieltags beendet waren.
/// </summary>
public class PredictionService(GameService gameService, AppStateService appStateService)
{
    /// <summary>Unterhalb dieser Anzahl beendeter Spiele ist das Modell nicht aussagekräftig.</summary>
    private const int MinTrainingGames = 10;

    /// <summary>Anzahl simulierter Saisonverläufe. Die Wahrscheinlichkeiten schwanken damit um weniger als einen Prozentpunkt.</summary>
    public const int SimulationRuns = 10_000;
    private const int SimulationChunk = 500;

    private sealed class SeasonCache((int Season, int Previous) version)
    {
        public (int Season, int Previous) Version { get; } = version;
        public Dictionary<int, MatchDayForecast> Forecasts { get; } = new();
        public SeasonSimulation? Simulation { get; set; }
    }

    private readonly Dictionary<(string League, int Season), SeasonCache> _cache = new();

    public async Task<MatchDayForecast> GetMatchDayAsync(int season, int matchDay)
    {
        var data = await LoadAsync(season);
        return GetMatchDay(data, matchDay);
    }

    /// <summary>Prognose eines Spiels mit Modell und Datengrundlage. Null, wenn das Spiel unbekannt ist oder keine Prognose hat.</summary>
    public async Task<MatchExplanation?> GetExplanationAsync(int season, int matchId)
    {
        var data = await LoadAsync(season);
        if (data.Games.FirstOrDefault(g => g.MatchId == matchId) is not { } game)
            return null;

        var forecast = GetMatchDay(data, game.MatchDay);
        if (forecast.Model is not { } model || forecast.Predictions.FirstOrDefault(p => p.Game.MatchId == matchId) is not { } prediction)
            return null;

        var cutoff = data.Games.Where(g => g.MatchDay == game.MatchDay).Min(g => g.KickoffUtc);
        var training = TrainingGames(data, cutoff).ToList();
        List<(Game, double)> GamesOf(int teamId) => training
            .Where(g => g.Involves(teamId))
            .OrderByDescending(g => g.KickoffUtc)
            .Select(g => (g, PredictionModel.GetWeight(g.KickoffUtc, cutoff)))
            .ToList();

        return new MatchExplanation
        {
            Prediction = prediction,
            Model = model,
            Cutoff = cutoff,
            HomeGames = GamesOf(game.Team1.Id),
            AwayGames = GamesOf(game.Team2.Id)
        };
    }

    /// <summary>Bewertet das Modell an allen beendeten Spielen der Saison. Gibt nach jedem Spieltag die Oberfläche frei.</summary>
    public async Task<PredictionBacktest> GetBacktestAsync(int season, CancellationToken cancellationToken = default)
    {
        var data = await LoadAsync(season);
        var predictions = new List<MatchPrediction>();
        var finishedMatchDays = data.Games.Where(g => g.MatchIsFinished).Select(g => g.MatchDay).Distinct().Order();
        foreach (var matchDay in finishedMatchDays)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cached = data.Cache.Forecasts.ContainsKey(matchDay);
            predictions.AddRange(GetMatchDay(data, matchDay).Predictions.Where(p => p.ActualScore != null));
            if (!cached)
                await Task.Delay(1, cancellationToken);
        }
        return new PredictionBacktest { Predictions = predictions };
    }

    /// <summary>
    /// Simuliert die offenen Spiele der Tabelle (bzw. Ligaphase) mit dem Modell nach dem aktuellen Stand.
    /// Null, wenn noch zu wenige Spiele beendet sind.
    /// </summary>
    public async Task<SeasonSimulation?> GetSimulationAsync(int season, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var data = await LoadAsync(season);
        if (data.Cache.Simulation is { } cached)
            return cached;

        var competition = appStateService.SelectedCompetition;
        var tableGames = data.Games.Where(competition.IsLeaguePhase).ToList();
        if (tableGames.Count == 0 || FitModel(data, DateTime.UtcNow) is not { } model)
            return null;

        var simulator = new SeasonSimulator(tableGames, model.Predict, seed: season);
        while (simulator.Runs < SimulationRuns && simulator.RemainingGames > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            simulator.Run(SimulationChunk);
            progress?.Report((double)simulator.Runs / SimulationRuns);
            await Task.Delay(1, cancellationToken);
        }
        if (simulator.RemainingGames == 0)
            simulator.Run(1);

        return data.Cache.Simulation = simulator.GetResult();
    }

    /// <summary>Nächster Spieltag mit noch nicht beendeten Spielen, sonst der letzte.</summary>
    public async Task<int> GetNextMatchDayAsync(int season)
    {
        var games = await gameService.GetSeasonGamesAsync(season);
        var open = games.Where(g => !g.MatchIsFinished && g.MatchDay > 0).Select(g => g.MatchDay).DefaultIfEmpty(0).Min();
        return open > 0 ? open : Math.Max(1, GameService.GetPlayedMatchDays(games));
    }

    private sealed record SeasonData(List<Game> Games, List<Game> PreviousGames, HashSet<int> NewTeams, SeasonCache Cache);

    private async Task<SeasonData> LoadAsync(int season)
    {
        var games = await gameService.GetSeasonGamesAsync(season);
        var previousGames = new List<Game>();
        if (season - 1 >= appStateService.SelectedCompetition.FirstSeason)
        {
            try
            {
                previousGames = await gameService.GetSeasonGamesAsync(season - 1);
            }
            catch (HttpRequestException)
            {
                // Ohne Vorsaison wird nur mit der laufenden Saison gerechnet.
            }
        }

        // Nur Änderungen an dieser Saison oder der Vorsaison machen die Prognosen ungültig.
        var key = (appStateService.League, season);
        var version = (gameService.GetSeasonVersion(season), gameService.GetSeasonVersion(season - 1));
        if (!_cache.TryGetValue(key, out var cache) || cache.Version != version)
        {
            cache = new SeasonCache(version);
            _cache[key] = cache;
        }

        var previousTeams = previousGames.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).ToHashSet();
        var newTeams = previousTeams.Count == 0
            ? new HashSet<int>()
            : games.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).Where(id => !previousTeams.Contains(id)).ToHashSet();
        return new SeasonData(games, previousGames, newTeams, cache);
    }

    private static MatchDayForecast GetMatchDay(SeasonData data, int matchDay)
    {
        if (data.Cache.Forecasts.TryGetValue(matchDay, out var forecast))
            return forecast;

        var matchDayGames = data.Games.Where(g => g.MatchDay == matchDay).OrderBy(g => g.KickoffUtc).ToList();
        var model = matchDayGames.Count == 0 ? null : FitModel(data, matchDayGames.Min(g => g.KickoffUtc));
        return data.Cache.Forecasts[matchDay] = new MatchDayForecast
        {
            MatchDay = matchDay,
            Model = model,
            Predictions = model == null ? new List<MatchPrediction>() : matchDayGames.Select(model.Predict).ToList()
        };
    }

    /// <summary>Trainiert das Modell mit allen Spielen, die vor <paramref name="cutoff"/> angepfiffen und beendet wurden.</summary>
    private static PredictionModel? FitModel(SeasonData data, DateTime cutoff)
    {
        var samples = TrainingGames(data, cutoff)
            .Select(g => new PredictionModel.Sample(g.Team1.Id, g.Team2.Id, g.FinalResult!.PointsTeam1, g.FinalResult.PointsTeam2,
                PredictionModel.GetWeight(g.KickoffUtc, cutoff)))
            .ToList();

        if (samples.Count < MinTrainingGames)
            return null;

        var teamIds = data.Games.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).Distinct();
        return PredictionModel.Fit(samples, teamIds, data.NewTeams);
    }

    private static IEnumerable<Game> TrainingGames(SeasonData data, DateTime cutoff) =>
        data.PreviousGames.Concat(data.Games).Where(g => g.MatchIsFinished && g.FinalResult != null && g.KickoffUtc < cutoff);
}
