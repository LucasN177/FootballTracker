namespace FootballTracker.Core.Models.Statistics;

public enum MatchOutcome
{
    HomeWin,
    Draw,
    AwayWin
}

/// <summary>Wie aus den Wahrscheinlichkeiten ein Tipp wird.</summary>
public enum TipStrategy
{
    /// <summary>Ergebnis mit den meisten erwarteten Kicktipp-Punkten. Remis wird dabei fast nie getippt.</summary>
    KicktippOptimal,

    /// <summary>Bei ausgeglichenen Spielen das wahrscheinlichste Remis, sonst wie <see cref="KicktippOptimal"/>.</summary>
    WithDraws
}

/// <summary>Prognose eines Spiels: erwartete Tore, Ergebniswahrscheinlichkeiten und Tipp.</summary>
public class MatchPrediction
{
    /// <summary>
    /// Bei <see cref="TipStrategy.WithDraws"/> wird Remis getippt, wenn Heim- und Auswärtssieg weniger als
    /// 10 Prozentpunkte auseinanderliegen. Im Backtest (1. und 2. Bundesliga 2021/22 bis 2025/26) ergibt das
    /// etwa so viele Remis-Tipps wie tatsächliche Remis und kostet rund 0,07 Kicktipp-Punkte pro Spiel.
    /// </summary>
    public const double DrawMargin = 0.10;

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

    /// <summary>Wahrscheinlichstes Unentschieden.</summary>
    public (int Home, int Away) DrawTip { get; init; }

    public bool IsBalanced => Math.Abs(HomeWin - AwayWin) < DrawMargin;

    public (int Home, int Away) GetTip(TipStrategy strategy) =>
        strategy == TipStrategy.WithDraws && IsBalanced ? DrawTip : Tip;

    /// <summary>Ergebnis nach 90 Minuten, sofern das Spiel beendet ist.</summary>
    public (int Home, int Away)? ActualScore =>
        Game.MatchIsFinished && Game.FinalResult is { } result ? (result.PointsTeam1, result.PointsTeam2) : null;

    public MatchOutcome? ActualOutcome => ActualScore is { } score ? GetOutcome(score) : null;

    public bool IsTendencyHit(TipStrategy strategy) => ActualOutcome == GetOutcome(GetTip(strategy));
    public bool IsExactHit(TipStrategy strategy) => ActualScore == GetTip(strategy);
    public int? GetKicktippPoints(TipStrategy strategy) => ActualScore is { } score ? GetKicktippPoints(GetTip(strategy), score) : null;

    /// <summary>Erwartete Kicktipp-Punkte eines Tipps über alle möglichen Ergebnisse.</summary>
    public double GetExpectedPoints((int Home, int Away) tip)
    {
        double points = 0;
        for (var i = 0; i < ScoreProbabilities.GetLength(0); i++)
            for (var j = 0; j < ScoreProbabilities.GetLength(1); j++)
                points += ScoreProbabilities[i, j] * GetKicktippPoints(tip, (i, j));
        return points;
    }

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
