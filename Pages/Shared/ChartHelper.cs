namespace FootballTracker.Pages.Shared;

public static class ChartHelper
{
    /// <summary>Spieltags-Beschriftung, bei vielen Werten nur jede n-te, damit die Achse lesbar bleibt.</summary>
    public static string[] MatchDayLabels(int count, int every = 5) =>
        Enumerable.Range(1, count)
            .Select(i => i == 1 || i % every == 0 || i == count ? i.ToString() : string.Empty)
            .ToArray();
}
