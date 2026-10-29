using FootballTracker.Core.Models;

namespace FootballTracker.Pages.Shared;

public static class ChartHelper
{

    /// <summary>Achsenbeschriftung je Spieltag bzw. Pokalrunde.</summary>
    public static string[] RoundLabels(Competition competition, int count) => competition.RoundShortNames.Length > 0
        ? Enumerable.Range(1, count).Select(competition.RoundShortName).ToArray()
        : MatchDayLabels(count);

    /// <summary>Spieltags-Beschriftung, bei vielen Werten nur jede n-te, damit die Achse lesbar bleibt.</summary>
    public static string[] MatchDayLabels(int count, int every = 5) =>
        Enumerable.Range(1, count)
            .Select(i => i == 1 || i % every == 0 || i == count ? i.ToString() : string.Empty)
            .ToArray();
}
