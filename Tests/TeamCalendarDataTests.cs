using FootballTracker.Core.Interfaces.Infrastructure;
using FootballTracker.Core.Interfaces.Response;
using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Api_Response_Models;
using FootballTracker.Services;

namespace FootballTracker.Tests;

public class TeamCalendarDataTests
{
    /// <summary>Liefert für <c>getmatchesbyteamid</c> eine feste Liste; alle anderen Abfragen werden nicht gebraucht.</summary>
    private sealed class FakeRepository(List<Game>? teamGames) : IOpenLigaDataRepository
    {
        public int Calls { get; private set; }
        public (int Past, int Future) LastRange { get; private set; }

        public Task<IResponse<List<Game>>> GetMatchesByTeam(int teamId, int weekCountPast, int weekCountFuture)
        {
            Calls++;
            LastRange = (weekCountPast, weekCountFuture);
            return Task.FromResult(teamGames == null ? Response<List<Game>>.Failure("Keine Daten verfügbar") : Response<List<Game>>.Success(teamGames));
        }

        public Task<IResponse<List<Game>>> GetMatchData(string leagueShortcut, int year) => throw new NotSupportedException();
        public Task<IResponse<List<Game>>> GetMatchDay(string leagueShortcut, int year, int matchDay) => throw new NotSupportedException();
        public Task<IResponse<List<Game>>> GetAllMatchesPerSeasonPerTeam(string leagueShortcut, int year, string teamFilter) => throw new NotSupportedException();
        public Task<IResponse<List<Game>>> GetMatchesBetweenTeams(int teamId1, int teamId2) => throw new NotSupportedException();
        public Task<IResponse<Game>> GetNextMatchByTeam(int leagueId, int teamId) => throw new NotSupportedException();
        public Task<IResponse<Game>> GetLastMatchByTeam(int leagueId, int teamId) => throw new NotSupportedException();
        public Task<IResponse<Game>> GetNextLeagueMatch(string leagueShortcut) => throw new NotSupportedException();
        public Task<IResponse<Game>> GetLastLeagueMatch(string leagueShortcut) => throw new NotSupportedException();
        public Task<IResponse<Group>> GetCurrentGroup(string leagueShortcut) => throw new NotSupportedException();
        public Task<IResponse<List<ApiGoalGetterResponse>>> GetGoalGetters(string leagueShortcut, int year) => throw new NotSupportedException();
        public Task<IResponse<List<ApiTeamResponse>>> GetTeams(string leagueShortcut, int year) => throw new NotSupportedException();
        public Task<IResponse<List<ApiTeamResponse>>> GetTable(string leagueShortcut, int year) => throw new NotSupportedException();
    }

    private static Game Game(int id, string league, int season, int day)
    {
        var game = TestData.Open(1, 2, day);
        game.MatchId = id;
        game.LeagueShortcut = league;
        game.LeagueSeason = season;
        return game;
    }

    private static GameService CreateService(FakeRepository repository) =>
        new(repository, new AppStateService(new LocalStorageService(null!)));

    [Fact]
    public async Task Keeps_only_known_competitions_of_the_season()
    {
        var repository = new FakeRepository(
        [
            Game(1, "bl1", 2025, 3),
            Game(2, "dfb", 2025, 2),
            Game(3, "ucl", 2025, 4),
            Game(4, "blclaude", 2025, 1),   // von Nutzern angelegte Liga
            Game(5, "ucl2026", 2025, 5),
            Game(6, "bl1", 2024, 1),        // Vorsaison
            Game(1, "bl1", 2025, 3)         // doppelt geliefert
        ]);

        var games = await CreateService(repository).GetTeamGamesAllCompetitionsAsync(1, 2025);

        Assert.Equal([2, 1, 3], games.Select(g => g.MatchId));
    }

    [Fact]
    public async Task No_content_means_no_games()
    {
        var games = await CreateService(new FakeRepository(null)).GetTeamGamesAllCompetitionsAsync(1, 2025);

        Assert.Empty(games);
    }

    [Fact]
    public async Task Past_seasons_are_requested_only_once()
    {
        var repository = new FakeRepository([Game(1, "bl1", 2021, 1)]);
        var service = CreateService(repository);

        await service.GetTeamGamesAllCompetitionsAsync(1, 2021);
        await service.GetTeamGamesAllCompetitionsAsync(1, 2021);

        Assert.Equal(1, repository.Calls);
    }

    [Fact]
    public void Week_range_covers_the_whole_season()
    {
        var today = new DateTime(2026, 11, 2);

        var (past, future) = GameService.GetSeasonWeekRange(2026, today);
        Assert.True(today.AddDays(-7 * past) <= new DateTime(2026, 7, 1));
        Assert.True(today.AddDays(7 * future) >= new DateTime(2027, 6, 30));

        // Abgeschlossene Saison: nur zurückblicken
        var (pastOld, futureOld) = GameService.GetSeasonWeekRange(2023, today);
        Assert.True(today.AddDays(-7 * pastOld) <= new DateTime(2023, 7, 1));
        Assert.Equal(1, futureOld);
    }

    [Theory]
    [InlineData("bl1", "Bundesliga")]
    [InlineData("DFB", "DFB-Pokal")]
    [InlineData("ucl", "Champions League")]
    public void Finds_competition_by_shortcut(string shortcut, string expected)
    {
        Assert.Equal(expected, Competition.FromShortcut(shortcut)?.ShortName);
    }

    [Theory]
    [InlineData("ucl2026")]
    [InlineData("tst1")]
    [InlineData(null)]
    public void Unknown_shortcuts_are_ignored(string? shortcut)
    {
        Assert.Null(Competition.FromShortcut(shortcut));
    }
}
