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

    /// <summary>Markenfarbe des Wettbewerbs (App-Leiste, Akzente, Diagramme).</summary>
    public required string PrimaryColor { get; init; }

    /// <summary>Dunklere Variante für Verläufe.</summary>
    public required string PrimaryDarkColor { get; init; }

    /// <summary>Hellere Variante für Verläufe und den dunklen Modus.</summary>
    public required string PrimaryLightColor { get; init; }

    public required string LogoUrl { get; init; }

    /// <summary>Breites Wortmarken-Logo statt quadratischem Emblem.</summary>
    public bool LogoIsWordmark { get; init; }

    private static readonly string[] BaseChartColors = ["#d20a11", "#1976d2", "#2e7d32", "#ed8b00", "#7c3aed", "#0891b2", "#be185d", "#4b5563"];

    /// <summary>Diagrammfarben: Markenfarbe zuerst, danach gut unterscheidbare Farben ohne ähnlichen Farbton.</summary>
    public string[] ChartColors => [PrimaryColor, .. BaseChartColors.Where(c => c != SimilarChartColor)];

    private string SimilarChartColor => Shortcut switch
    {
        "bl2" => "#1976d2",
        "dfb" => "#2e7d32",
        _ => "#d20a11"
    };

    public static readonly Competition Bundesliga = new()
    {
        Shortcut = "bl1",
        Name = "1. Fußball-Bundesliga",
        ShortName = "Bundesliga",
        IsCup = false,
        RoundCount = 34,
        PrimaryColor = "#d20a11",
        PrimaryDarkColor = "#8c070b",
        PrimaryLightColor = "#ff5252",
        LogoUrl = "Resources/img/Bundesliga-Logo.jpeg"
    };

    public static readonly Competition ZweiteBundesliga = new()
    {
        Shortcut = "bl2",
        Name = "2. Fußball-Bundesliga",
        ShortName = "2. Bundesliga",
        IsCup = false,
        RoundCount = 34,
        PrimaryColor = "#1360b8",
        PrimaryDarkColor = "#0a3a75",
        PrimaryLightColor = "#4f9cf0",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/5/58/2._Bundesliga_Wordmark.svg",
        LogoIsWordmark = true
    };

    public static readonly Competition DfbPokal = new()
    {
        Shortcut = "dfb",
        Name = "DFB-Pokal",
        ShortName = "DFB-Pokal",
        IsCup = true,
        RoundCount = 6,
        FirstSeason = 2023,
        RoundNames = ["1. Runde", "2. Runde", "Achtelfinale", "Viertelfinale", "Halbfinale", "Finale"],
        PrimaryColor = "#00824a",
        PrimaryDarkColor = "#00502d",
        PrimaryLightColor = "#3ccf8e",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Deutscher_Fu%C3%9Fball-Bund_Logo_2025%E2%80%93.svg"
    };

    public static IReadOnlyList<Competition> All { get; } = [Bundesliga, ZweiteBundesliga, DfbPokal];
}
