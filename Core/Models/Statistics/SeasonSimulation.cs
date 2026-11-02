namespace FootballTracker.Core.Models.Statistics;

/// <summary>Ergebnis einer Monte-Carlo-Simulation der restlichen Saison.</summary>
public class SeasonSimulation
{
    public int Runs { get; init; }
    public int RemainingGames { get; init; }

    /// <summary>Teams sortiert nach erwartetem Tabellenplatz.</summary>
    public required List<SimulatedTeam> Teams { get; init; }
}

public class SimulatedTeam
{
    public required Team Team { get; init; }
    public int Points { get; init; }
    public int GoalDifference { get; init; }
    public int Played { get; init; }
    public double ExpectedPoints { get; init; }
    public double ExpectedPosition { get; init; }

    /// <summary>Wahrscheinlichkeit je Endplatz, Index 0 = Platz 1.</summary>
    public required double[] PositionProbabilities { get; init; }

    /// <summary>Wahrscheinlichkeit, auf einem der Plätze <paramref name="from"/> bis <paramref name="to"/> zu landen.</summary>
    public double GetProbability(int from, int to) =>
        PositionProbabilities.Skip(from - 1).Take(Math.Max(0, to - from + 1)).Sum();
}
