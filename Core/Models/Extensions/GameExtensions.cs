using MudBlazor;

namespace FootballTracker.Core.Models.Extensions;

public enum GameStatus
{
    Scheduled,
    Live,
    Pending,
    Finished
}

public static class GameExtensions
{
    public static string GetFormattedDateTime(this Game game, string format = "dd.MM.yyyy HH:mm")
    {
        return game.MatchDateTime.ToString(format);
    }

    public static GameStatus GetStatus(this Game game)
    {
        if (game.MatchIsFinished)
            return GameStatus.Finished;
        if (game.IsLive)
            return GameStatus.Live;
        return game.HasStarted ? GameStatus.Pending : GameStatus.Scheduled;
    }

    public static string GetGameStatus(this Game game) => game.GetStatus() switch
    {
        GameStatus.Finished => "Beendet",
        GameStatus.Live => "Live",
        GameStatus.Pending => "Ausstehend",
        _ => "Angesetzt"
    };

    public static Color GetStatusColor(this Game game) => game.GetStatus() switch
    {
        GameStatus.Finished => Color.Success,
        GameStatus.Live => Color.Error,
        GameStatus.Pending => Color.Warning,
        _ => Color.Info
    };

    public static string GetScoreDisplay(this Game game, string separator = " : ")
    {
        var score = game.CurrentScore;
        if (score != null)
            return $"{score.Value.Team1}{separator}{score.Value.Team2}";

        return game.IsUpcoming ? game.GetFormattedDateTime("HH:mm") : $"-{separator}-";
    }

    public static bool Involves(this Game game, int teamId) => game.Team1.Id == teamId || game.Team2.Id == teamId;

    public static bool IsHomeTeam(this Game game, int teamId) => game.Team1.Id == teamId;

    public static Team GetOpponent(this Game game, int teamId) => game.Team1.Id == teamId ? game.Team2 : game.Team1;

    /// <summary>Ergebnis aus Sicht eines Teams: 'S', 'U', 'N' oder null, falls noch nicht beendet.</summary>
    public static char? GetOutcomeFor(this Game game, int teamId)
    {
        if (!game.MatchIsFinished || game.FinalResult == null || !game.Involves(teamId))
            return null;

        var (own, other) = game.GetGoalsFor(teamId);
        return own > other ? 'S' : own == other ? 'U' : 'N';
    }

    public static (int Own, int Other) GetGoalsFor(this Game game, int teamId)
    {
        var result = game.FinalResult;
        if (result == null)
            return (0, 0);
        return game.IsHomeTeam(teamId)
            ? (result.PointsTeam1, result.PointsTeam2)
            : (result.PointsTeam2, result.PointsTeam1);
    }

    public static int GetPointsFor(this Game game, int teamId) => game.GetOutcomeFor(teamId) switch
    {
        'S' => 3,
        'U' => 1,
        _ => 0
    };

    public static Color GetOutcomeColor(char? outcome) => outcome switch
    {
        'S' => Color.Success,
        'U' => Color.Warning,
        'N' => Color.Error,
        _ => Color.Default
    };

    public static string GetTimeUntil(this Game game)
    {
        var timeUntil = game.MatchDateTimeUtc - DateTime.UtcNow;
        if (timeUntil <= TimeSpan.Zero)
            return string.Empty;
        if (timeUntil.TotalDays >= 1)
            return $"in {(int)timeUntil.TotalDays} T";
        if (timeUntil.TotalHours >= 1)
            return $"in {(int)timeUntil.TotalHours} Std";
        return timeUntil.TotalMinutes >= 1 ? $"in {(int)timeUntil.TotalMinutes} Min" : "gleich";
    }
}
