using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Extensions;

namespace FootballTracker.Services;

/// <summary>Spieldaten des im <see cref="AppStateService"/> gewählten Wettbewerbs.</summary>
public class GameService(IOpenLigaDataRepository openLigaDataRepository, AppStateService appStateService)
{
    /// <summary>Wie lange Daten der laufenden Saison zwischengespeichert werden. Abgeschlossene Saisons werden dauerhaft gecacht.</summary>
    private static readonly TimeSpan CurrentSeasonCacheDuration = TimeSpan.FromMinutes(2);

    private readonly Dictionary<(string League, int Season), (DateTime LoadedAt, List<Game> Games)> _seasonCache = new();
    private readonly Dictionary<string, (DateTime LoadedAt, int MatchDay)> _currentMatchDay = new();

    public DateTime? LastUpdate { get; private set; }

    /// <summary>Wird bei jeder Datenänderung erhöht, damit abgeleitete Statistiken ihren Cache invalidieren können.</summary>
    public int Version { get; private set; }

    /// <summary>Anzahl der Spieltage bzw. Runden des gewählten Wettbewerbs.</summary>
    public int MatchDaysPerSeason => appStateService.SelectedCompetition.RoundCount;

    private string League => appStateService.League;

    public async Task<List<Game>> GetSeasonGamesAsync(int season, bool forceRefresh = false)
    {
        var key = (League, season);
        if (!forceRefresh && _seasonCache.TryGetValue(key, out var cached) && !IsExpired(season, cached.LoadedAt))
            return cached.Games;

        var games = Unwrap(await openLigaDataRepository.GetMatchData(key.League, season));
        _seasonCache[key] = (DateTime.Now, games);
        LastUpdate = DateTime.Now;
        Version++;
        return games;
    }

    public async Task<List<Game>> GetMatchDataAsync(int season, int matchDay, bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            // Nur den Spieltag frisch laden (deutlich kleinere Antwort als die ganze Saison).
            var fresh = Unwrap(await openLigaDataRepository.GetMatchDay(League, season, matchDay));
            if (_seasonCache.TryGetValue((League, season), out var cached))
            {
                cached.Games.RemoveAll(g => g.MatchDay == matchDay);
                cached.Games.AddRange(fresh);
            }
            LastUpdate = DateTime.Now;
            Version++;
            return fresh.OrderBy(g => g.MatchDateTimeUtc).ToList();
        }

        var games = await GetSeasonGamesAsync(season);
        return games.Where(g => g.MatchDay == matchDay).OrderBy(g => g.MatchDateTimeUtc).ToList();
    }

    /// <summary>Aktueller Spieltag der Saison. Für vergangene Saisons der letzte Spieltag.</summary>
    public async Task<int> GetCurrentMatchDay(int season)
    {
        if (season != AppStateService.CurrentSeasonYear)
            return MatchDaysPerSeason;

        if (_currentMatchDay.TryGetValue(League, out var cached) && DateTime.Now - cached.LoadedAt < CurrentSeasonCacheDuration)
            return cached.MatchDay;

        var result = await openLigaDataRepository.GetCurrentGroup(League);
        var matchDay = Math.Clamp(result is { IsSuccess: true, Data: not null } ? result.Data.GroupOrderId : 1, 1, MatchDaysPerSeason);
        _currentMatchDay[League] = (DateTime.Now, matchDay);
        return matchDay;
    }

    /// <summary>Anzahl Spieltage, an denen mindestens ein Spiel beendet wurde.</summary>
    public static int GetPlayedMatchDays(IEnumerable<Game> games) =>
        games.Where(g => g.MatchIsFinished).Select(g => g.MatchDay).DefaultIfEmpty(0).Max();

    public async Task<List<Game>> GetTeamGamesAsync(int season, int teamId)
    {
        var games = await GetSeasonGamesAsync(season);
        return games.Where(g => g.Involves(teamId)).OrderBy(g => g.MatchDateTimeUtc).ToList();
    }

    public async Task<List<Game>> GetHeadToHeadAsync(int teamId1, int teamId2)
    {
        var games = Unwrap(await openLigaDataRepository.GetMatchesBetweenTeams(teamId1, teamId2));
        return games.Where(g => g.MatchIsFinished).OrderByDescending(g => g.MatchDateTimeUtc).ToList();
    }

    public async Task<List<Game>> GetLiveAndTodayGamesAsync(bool forceRefresh = false)
    {
        var season = AppStateService.CurrentSeasonYear;
        var matchDay = await GetCurrentMatchDay(season);
        return await GetMatchDataAsync(season, matchDay, forceRefresh);
    }

    private static bool IsExpired(int season, DateTime loadedAt) =>
        season == AppStateService.CurrentSeasonYear && DateTime.Now - loadedAt > CurrentSeasonCacheDuration;

    private static T Unwrap<T>(IResponse<T> response)
    {
        if (response is { IsSuccess: true, Data: not null })
            return response.Data;
        throw new HttpRequestException(response.ErrorMessage ?? response.Exception?.Message ?? "Daten konnten nicht geladen werden.", response.Exception);
    }
}
