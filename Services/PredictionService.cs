using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Services;

/// <summary>Prognose eines Spieltags mit dem Modell, das auf allen vorher beendeten Spielen trainiert wurde.</summary>
public class MatchDayForecast
{
    public int MatchDay { get; init; }
    public PredictionModel? Model { get; init; }
    public List<MatchPrediction> Predictions { get; init; } = new();
}

/// <summary>
/// Berechnet Spielprognosen mit dem <see cref="PredictionModel"/>. Trainiert wird mit der laufenden Saison und der Vorsaison
/// desselben Wettbewerbs, jeweils nur mit Spielen, die vor dem ersten Anstoß des Spieltags beendet waren.
/// </summary>
public class PredictionService(GameService gameService, AppStateService appStateService)
{
    /// <summary>Unterhalb dieser Anzahl beendeter Spiele ist das Modell nicht aussagekräftig.</summary>
    private const int MinTrainingGames = 10;

    private readonly Dictionary<(string League, int Season), (int Version, Dictionary<int, MatchDayForecast> MatchDays)> _cache = new();

    public async Task<MatchDayForecast> GetMatchDayAsync(int season, int matchDay)
    {
        var data = await LoadAsync(season);
        return GetMatchDay(data, matchDay);
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
            var cached = data.Forecasts.ContainsKey(matchDay);
            predictions.AddRange(GetMatchDay(data, matchDay).Predictions.Where(p => p.ActualScore != null));
            if (!cached)
                await Task.Delay(1, cancellationToken);
        }
        return new PredictionBacktest { Predictions = predictions };
    }

    /// <summary>Nächster Spieltag mit noch nicht beendeten Spielen, sonst der letzte.</summary>
    public async Task<int> GetNextMatchDayAsync(int season)
    {
        var games = await gameService.GetSeasonGamesAsync(season);
        var open = games.Where(g => !g.MatchIsFinished && g.MatchDay > 0).Select(g => g.MatchDay).DefaultIfEmpty(0).Min();
        return open > 0 ? open : Math.Max(1, GameService.GetPlayedMatchDays(games));
    }

    private sealed record SeasonData(List<Game> Games, List<Game> PreviousGames, HashSet<int> NewTeams, Dictionary<int, MatchDayForecast> Forecasts);

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

        var key = (appStateService.League, season);
        if (!_cache.TryGetValue(key, out var cached) || cached.Version != gameService.Version)
        {
            cached = (gameService.Version, new Dictionary<int, MatchDayForecast>());
            _cache[key] = cached;
        }

        var previousTeams = previousGames.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).ToHashSet();
        var newTeams = previousTeams.Count == 0
            ? new HashSet<int>()
            : games.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).Where(id => !previousTeams.Contains(id)).ToHashSet();
        return new SeasonData(games, previousGames, newTeams, cached.MatchDays);
    }

    private static MatchDayForecast GetMatchDay(SeasonData data, int matchDay)
    {
        if (data.Forecasts.TryGetValue(matchDay, out var forecast))
            return forecast;

        var matchDayGames = data.Games.Where(g => g.MatchDay == matchDay).OrderBy(g => g.KickoffUtc).ToList();
        if (matchDayGames.Count == 0)
            return data.Forecasts[matchDay] = new MatchDayForecast { MatchDay = matchDay };

        var cutoff = matchDayGames.Min(g => g.KickoffUtc);
        var samples = data.PreviousGames.Concat(data.Games)
            .Where(g => g.MatchIsFinished && g.FinalResult != null && g.KickoffUtc < cutoff)
            .Select(g => new PredictionModel.Sample(g.Team1.Id, g.Team2.Id, g.FinalResult!.PointsTeam1, g.FinalResult.PointsTeam2,
                PredictionModel.GetWeight(g.KickoffUtc, cutoff)))
            .ToList();

        if (samples.Count < MinTrainingGames)
            return data.Forecasts[matchDay] = new MatchDayForecast { MatchDay = matchDay };

        var teamIds = data.Games.SelectMany(g => new[] { g.Team1.Id, g.Team2.Id }).Distinct();
        var model = PredictionModel.Fit(samples, teamIds, data.NewTeams);
        return data.Forecasts[matchDay] = new MatchDayForecast
        {
            MatchDay = matchDay,
            Model = model,
            Predictions = matchDayGames.Select(model.Predict).ToList()
        };
    }
}
