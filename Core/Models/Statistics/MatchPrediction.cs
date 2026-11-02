namespace FootballTracker.Core.Models.Statistics;

public enum MatchOutcome
{
    HomeWin,
    Draw,
    AwayWin
}

/// <summary>Prognose eines Spiels: erwartete Tore, Ergebniswahrscheinlichkeiten und Tipp.</summary>
public class MatchPrediction
{
    public required Game Game { get; init; }
    public double ExpectedGoalsHome { get; init; }
    public double ExpectedGoalsAway { get; init; }

    /// <summary>Wahrscheinlichkeit je Ergebnis, Index [Heimtore, Auswärtstore].</summary>
    public required double[,] ScoreProbabilities { get; init; }

    public double HomeWin { get; init; }
    public double Draw { get; init; }
    public double AwayWin { get; init; }
    public (int Home, int Away) MostLikelyScore { get; init; }
    public double MostLikelyProbability { get; init; }

    /// <summary>Tipp mit den meisten erwarteten Kicktipp-Punkten (nicht zwingend das wahrscheinlichste Ergebnis).</summary>
    public (int Home, int Away) Tip { get; init; }
    public double TipExpectedPoints { get; init; }

    public MatchOutcome PredictedOutcome =>
        HomeWin >= Draw && HomeWin >= AwayWin ? MatchOutcome.HomeWin : AwayWin > Draw ? MatchOutcome.AwayWin : MatchOutcome.Draw;

    /// <summary>Ergebnis nach 90 Minuten, sofern das Spiel beendet ist.</summary>
    public (int Home, int Away)? ActualScore =>
        Game.MatchIsFinished && Game.FinalResult is { } result ? (result.PointsTeam1, result.PointsTeam2) : null;

    public MatchOutcome? ActualOutcome => ActualScore is { } score ? GetOutcome(score) : null;
    public bool IsTendencyHit => ActualOutcome == PredictedOutcome;
    public bool IsExactHit => ActualScore == Tip;
    public int? KicktippPoints => ActualScore is { } score ? GetKicktippPoints(Tip, score) : null;

    public double GetProbability(MatchOutcome outcome) => outcome switch
    {
        MatchOutcome.HomeWin => HomeWin,
        MatchOutcome.Draw => Draw,
        _ => AwayWin
    };

    /// <summary>Brier-Score über Heimsieg/Remis/Auswärtssieg (0 = perfekt, Raten mit je 1/3 = 0,667).</summary>
    public double? BrierScore => ActualOutcome is { } actual
        ? Enum.GetValues<MatchOutcome>().Sum(o => Math.Pow(GetProbability(o) - (o == actual ? 1 : 0), 2))
        : null;

    public static MatchOutcome GetOutcome((int Home, int Away) score) =>
        score.Home > score.Away ? MatchOutcome.HomeWin : score.Home == score.Away ? MatchOutcome.Draw : MatchOutcome.AwayWin;

    /// <summary>Kicktipp-Standardwertung: 4 exakt, 3 Tordifferenz, 2 Tendenz (bei Remis gibt es keine Tordifferenz-Punkte).</summary>
    public static int GetKicktippPoints((int Home, int Away) tip, (int Home, int Away) result)
    {
        if (tip == result)
            return 4;
        if (GetOutcome(tip) != GetOutcome(result))
            return 0;
        return tip.Home != tip.Away && tip.Home - tip.Away == result.Home - result.Away ? 3 : 2;
    }
}
