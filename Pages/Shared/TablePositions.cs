namespace FootballTracker.Pages.Shared;

/// <summary>Farbliche Zonen der Bundesliga-Tabelle.</summary>
public static class TablePositions
{
    public static string GetZone(int position, int teamCount = 18) => position switch
    {
        <= 4 => "cl",
        5 => "el",
        6 => "ecl",
        _ when teamCount >= 18 && position == teamCount - 2 => "rel",
        _ when teamCount >= 18 && position > teamCount - 2 => "down",
        _ => "none"
    };

    public static readonly (string Zone, string Label)[] Legend =
    [
        ("cl", "Champions League"),
        ("el", "Europa League"),
        ("ecl", "Conference League"),
        ("rel", "Relegation"),
        ("down", "Abstieg")
    ];
}
