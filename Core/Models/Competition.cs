namespace FootballTracker.Core.Models;

/// <summary>Ein von OpenLigaDB unterstützter Wettbewerb (Liga, Pokal oder Liga mit K.-o.-Phase).</summary>
public class Competition
{
    public required string Shortcut { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required bool IsCup { get; init; }

    /// <summary>Anzahl der Spieltage bzw. Runden einer Saison.</summary>
    public required int RoundCount { get; init; }

    /// <summary>
    /// Anzahl der Spieltage mit Tabelle (z.B. Ligaphase der Champions League). 0 = alle Runden (reine Liga).
    /// Pokale haben keine Tabelle.
    /// </summary>
    public int LeaguePhaseRounds { get; init; }

    /// <summary>Erste Saison, für die OpenLigaDB Daten liefert.</summary>
    public int FirstSeason { get; init; } = 2020;

    /// <summary>Rundennamen (Index 0 = 1. Runde). Leer = "n. Spieltag".</summary>
    public string[] RoundNames { get; init; } = [];

    /// <summary>Kurze Rundennamen für Auswahlleisten und Diagrammachsen.</summary>
    public string[] RoundShortNames { get; init; } = [];

    public bool IsLeague => !IsCup;

    /// <summary>Spieltage, die in die Tabelle eingehen.</summary>
    public int TableRounds => IsCup ? 0 : LeaguePhaseRounds > 0 ? LeaguePhaseRounds : RoundCount;

    /// <summary>Wettbewerb mit K.-o.-Runden (Pokal oder Champions League nach der Ligaphase).</summary>
    public bool HasKnockout => TableRounds < RoundCount;

    /// <summary>Spiel zählt für die Tabelle (Liga bzw. Ligaphase).</summary>
    public bool IsLeaguePhase(Game game) => game.MatchDay >= 1 && game.MatchDay <= TableRounds;

    /// <summary>Bezeichnung einer Runde, z.B. "8. Spieltag" oder "Achtelfinale".</summary>
    public string RoundName(int round) =>
        round >= 1 && round <= RoundNames.Length ? RoundNames[round - 1] : $"{round}. Spieltag";

    /// <summary>Kurze Beschriftung für Auswahlleisten, z.B. "8" oder "AF".</summary>
    public string RoundShortName(int round) =>
        round >= 1 && round <= RoundShortNames.Length ? RoundShortNames[round - 1] : round.ToString();

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

    /// <summary>Kurzform der Wortmarke für schmale Bildschirme, z.B. "2.".</summary>
    public string LogoShortText { get; init; } = string.Empty;

    private static readonly string[] BaseChartColors = ["#d20a11", "#1976d2", "#2e7d32", "#ed8b00", "#7c3aed", "#0891b2", "#be185d", "#4b5563"];

    /// <summary>Basisfarbe, die der Markenfarbe zu ähnlich ist und in Diagrammen entfällt.</summary>
    private string SimilarChartColor { get; init; } = "#d20a11";

    /// <summary>Diagrammfarben: Markenfarbe zuerst, danach gut unterscheidbare Farben ohne ähnlichen Farbton.</summary>
    public string[] ChartColors => [PrimaryColor, .. BaseChartColors.Where(c => c != SimilarChartColor)];

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
        SimilarChartColor = "#1976d2",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/5/58/2._Bundesliga_Wordmark.svg",
        LogoIsWordmark = true,
        LogoShortText = "2."
    };

    public static readonly Competition DritteLiga = new()
    {
        Shortcut = "bl3",
        Name = "3. Liga",
        ShortName = "3. Liga",
        IsCup = false,
        RoundCount = 38,
        PrimaryColor = "#34414e",
        PrimaryDarkColor = "#1a222b",
        PrimaryLightColor = "#90a4b7",
        SimilarChartColor = "#4b5563",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/e/ec/3._Liga_Wordmark.svg",
        LogoIsWordmark = true,
        LogoShortText = "3."
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
        RoundShortNames = ["1. R.", "2. R.", "AF", "VF", "HF", "Finale"],
        PrimaryColor = "#00824a",
        PrimaryDarkColor = "#00502d",
        PrimaryLightColor = "#3ccf8e",
        SimilarChartColor = "#2e7d32",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Deutscher_Fu%C3%9Fball-Bund_Logo_2025%E2%80%93.svg"
    };

    public static readonly Competition ChampionsLeague = new()
    {
        Shortcut = "ucl",
        Name = "UEFA Champions League",
        ShortName = "Champions League",
        IsCup = false,
        RoundCount = 16,
        LeaguePhaseRounds = 8,
        FirstSeason = 2025,
        RoundNames =
        [
            "1. Spieltag", "2. Spieltag", "3. Spieltag", "4. Spieltag", "5. Spieltag", "6. Spieltag", "7. Spieltag", "8. Spieltag",
            "Playoffs", "Achtelfinale (Hinspiel)", "Achtelfinale (Rückspiel)", "Viertelfinale (Hinspiel)", "Viertelfinale (Rückspiel)",
            "Halbfinale (Hinspiel)", "Halbfinale (Rückspiel)", "Finale"
        ],
        RoundShortNames = ["1", "2", "3", "4", "5", "6", "7", "8", "PO", "AF 1", "AF 2", "VF 1", "VF 2", "HF 1", "HF 2", "Finale"],
        PrimaryColor = "#0b1f5c",
        PrimaryDarkColor = "#040b26",
        PrimaryLightColor = "#6d8cff",
        SimilarChartColor = "#1976d2",
        LogoUrl = "https://upload.wikimedia.org/wikipedia/commons/d/d1/UEFA_Champions_League_logo_no_text.svg"
    };

    public static IReadOnlyList<Competition> All { get; } = [Bundesliga, ZweiteBundesliga, DritteLiga, DfbPokal, ChampionsLeague];
}
