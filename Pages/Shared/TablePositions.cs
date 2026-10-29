using FootballTracker.Core.Models;

namespace FootballTracker.Pages.Shared;

/// <summary>Farbliche Zonen der Tabellen je Wettbewerb.</summary>
public static class TablePositions
{
    public static string GetZone(Competition competition, int position, int teamCount = 18)
    {
        if (competition == Competition.ChampionsLeague)
        {
            return position switch
            {
                <= 8 => "ko",
                <= 24 => "po",
                _ => "none"
            };
        }

        // Abstiegszonen gelten nur bei vollständiger Tabelle
        var fullTable = teamCount >= 18;
        if (competition == Competition.DritteLiga)
        {
            if (fullTable && position > teamCount - 4)
                return "down";
        }
        else
        {
            if (fullTable && position == teamCount - 2)
                return "rel";
            if (fullTable && position > teamCount - 2)
                return "down";
        }

        if (competition == Competition.ZweiteBundesliga || competition == Competition.DritteLiga)
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

    public static (string Zone, string Label)[] Legend(Competition competition)
    {
        if (competition == Competition.ChampionsLeague)
            return [("ko", "Achtelfinale"), ("po", "Playoffs")];

        if (competition == Competition.DritteLiga)
            return [("up", "Aufstieg"), ("relup", "Relegation (Aufstieg)"), ("down", "Abstieg")];

        if (competition == Competition.ZweiteBundesliga)
        {
            return
            [
                ("up", "Aufstieg"),
                ("relup", "Relegation (Aufstieg)"),
                ("rel", "Relegation (Abstieg)"),
                ("down", "Abstieg")
            ];
        }

        return
        [
            ("cl", "Champions League"),
            ("el", "Europa League"),
            ("ecl", "Conference League"),
            ("rel", "Relegation"),
            ("down", "Abstieg")
        ];
    }
}
