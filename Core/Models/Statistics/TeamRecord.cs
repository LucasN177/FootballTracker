namespace FootballTracker.Core.Models.Statistics;

/// <summary>Bilanz eines Teams über eine Menge von Spielen (z.B. Heim, Auswärts, gesamt).</summary>
public class TeamRecord
{
    public required Team Team { get; init; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Draw { get; set; }
    public int Lost { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int CleanSheets { get; set; }
    public int Points => Won * 3 + Draw;
    public int GoalDifference => GoalsFor - GoalsAgainst;
    public double PointsPerGame => Played == 0 ? 0 : (double)Points / Played;
    public double WinRate => Played == 0 ? 0 : (double)Won / Played * 100;
}
