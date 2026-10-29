using FootballTracker.Core.Models;

namespace FootballTracker.Pages.Shared;

/// <summary>Farbliche Zonen der Ligatabellen.</summary>
public static class TablePositions
{
    public static string GetZone(Competition competition, int position, int teamCount = 18)
    {
        // Abstiegszonen gelten nur bei vollständiger Tabelle
        if (teamCount >= 18 && position == teamCount - 2)
            return "rel";
        if (teamCount >= 18 && position > teamCount - 2)
            return "down";

        if (competition == Competition.ZweiteBundesliga)
        {
            return position switch
            {
                <= 2 => "up",
                3 => "relup",
                _ => "none"
            };
        }

        return position switch
        {
            <= 4 => "cl",
            5 => "el",
            6 => "ecl",
            _ => "none"
        };
    }

    public static (string Zone, string Label)[] Legend(Competition competition) => competition == Competition.ZweiteBundesliga
        ?
        [
            ("up", "Aufstieg"),
            ("relup", "Relegation (Aufstieg)"),
            ("rel", "Relegation (Abstieg)"),
            ("down", "Abstieg")
        ]
        :
        [
            ("cl", "Champions League"),
            ("el", "Europa League"),
            ("ecl", "Conference League"),
            ("rel", "Relegation"),
            ("down", "Abstieg")
        ];
}
