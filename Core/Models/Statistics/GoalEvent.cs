namespace FootballTracker.Core.Models.Statistics;

/// <summary>Ein Tor inklusive Spiel- und Teamzuordnung.</summary>
public class GoalEvent
{
    public required Game Game { get; init; }
    public required Goal Goal { get; init; }

    /// <summary>Team, dem das Tor gutgeschrieben wurde.</summary>
    public required Team ScoringTeam { get; init; }

    /// <summary>Team des Torschützen (bei Eigentoren der Gegner des <see cref="ScoringTeam"/>).</summary>
    public required Team PlayerTeam { get; init; }

    public Team Opponent => Game.Team1.Id == PlayerTeam.Id ? Game.Team2 : Game.Team1;
    public int MatchDay => Game.MatchDay;
}
