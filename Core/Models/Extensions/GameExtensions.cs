namespace FootballTracker.Core.Models.Extensions;

public static class GameExtensions
{
    public static string GetFormattedDateTime(this Game game, string format = "dd.MM.yyyy HH:mm")
    {
        return game.MatchDateTime.ToString(format);
    }

    public static string GetGameStatus(this Game game)
    {
        if (game.MatchIsFinished)
            return "Beendet";
            
        if (game.MatchDateTime > DateTime.Now)
            return "Angesetzt";
            
        return "Live";
    }

    public static string GetScoreDisplay(this Game game)
    {
        if (game.FinalResult != null)
            return $"{game.FinalResult.PointsTeam1} : {game.FinalResult.PointsTeam2}";
            
        if (game.IsUpcoming)
            return game.GetFormattedDateTime("HH:mm");
            
        return "- : -";
    }
}