namespace FootballTracker.Pages.Shared;

public static class ChartHelper
{
    /// <summary>Farben für mehrere Datenreihen (Teams, Spieler) in Diagrammen.</summary>
    public static readonly string[] SeriesColors = ["#d20a11", "#1976d2", "#2e7d32", "#ed8b00", "#7c3aed", "#0891b2", "#be185d", "#4b5563"];

    /// <summary>Spieltags-Beschriftung, bei vielen Werten nur jede n-te, damit die Achse lesbar bleibt.</summary>
    public static string[] MatchDayLabels(int count, int every = 5) =>
        Enumerable.Range(1, count)
            .Select(i => i == 1 || i % every == 0 || i == count ? i.ToString() : string.Empty)
            .ToArray();
}
