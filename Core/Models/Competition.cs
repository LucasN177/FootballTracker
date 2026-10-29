namespace FootballTracker.Core.Models;

/// <summary>Ein von OpenLigaDB unterstützter Wettbewerb (Liga oder Pokal).</summary>
public class Competition
{
    public required string Shortcut { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required bool IsCup { get; init; }

    /// <summary>Anzahl der Spieltage bzw. Runden einer Saison.</summary>
    public required int RoundCount { get; init; }

    /// <summary>Erste Saison, für die OpenLigaDB Daten liefert.</summary>
    public int FirstSeason { get; init; } = 2020;

    /// <summary>Rundennamen bei Pokalwettbewerben (Index 0 = 1. Runde).</summary>
    public string[] RoundNames { get; init; } = [];

    public bool IsLeague => !IsCup;

    /// <summary>Bezeichnung einer Runde, z.B. "8. Spieltag" oder "Achtelfinale".</summary>
    public string RoundName(int round) =>
        IsCup && round >= 1 && round <= RoundNames.Length ? RoundNames[round - 1] : $"{round}. Spieltag";

    /// <summary>Kurze Beschriftung für Auswahlleisten, z.B. "8" oder "AF".</summary>
    public string RoundShortName(int round) => IsCup
        ? RoundName(round) switch
        {
            "Achtelfinale" => "AF",
            "Viertelfinale" => "VF",
            "Halbfinale" => "HF",
            "Finale" => "Finale",
            var name => name.Replace(". Runde", ". R.")
        }
        : round.ToString();

    public string RoundLabel => IsCup ? "Runde" : "Spieltag";

    public static readonly Competition Bundesliga = new()
    {
        Shortcut = "bl1",
        Name = "1. Fußball-Bundesliga",
        ShortName = "Bundesliga",
        IsCup = false,
        RoundCount = 34
    };

    public static readonly Competition ZweiteBundesliga = new()
    {
        Shortcut = "bl2",
        Name = "2. Fußball-Bundesliga",
        ShortName = "2. Bundesliga",
        IsCup = false,
        RoundCount = 34
    };

    public static readonly Competition DfbPokal = new()
    {
        Shortcut = "dfb",
        Name = "DFB-Pokal",
        ShortName = "DFB-Pokal",
        IsCup = true,
        RoundCount = 6,
        FirstSeason = 2023,
        RoundNames = ["1. Runde", "2. Runde", "Achtelfinale", "Viertelfinale", "Halbfinale", "Finale"]
    };

    public static IReadOnlyList<Competition> All { get; } = [Bundesliga, ZweiteBundesliga, DfbPokal];
}
