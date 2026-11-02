namespace FootballTracker.Core.Models.Statistics;

/// <summary>
/// Rückblickende Bewertung des Modells: Jedes beendete Spiel wurde nur mit den Daten prognostiziert,
/// die vor seinem Spieltag vorlagen.
/// </summary>
public class PredictionBacktest
{
    public required List<MatchPrediction> Predictions { get; init; }

    public int Games => Predictions.Count;
    public int TendencyHits => Predictions.Count(p => p.IsTendencyHit);
    public int ExactHits => Predictions.Count(p => p.IsExactHit);
    public int KicktippPoints => Predictions.Sum(p => p.KicktippPoints ?? 0);
    public double TendencyRate => Games == 0 ? 0 : (double)TendencyHits / Games;
    public double ExactRate => Games == 0 ? 0 : (double)ExactHits / Games;
    public double PointsPerGame => Games == 0 ? 0 : (double)KicktippPoints / Games;
    public double BrierScore => Games == 0 ? 0 : Predictions.Average(p => p.BrierScore ?? 0);

    /// <summary>Vergleichswert: Quote, wenn man immer auf Heimsieg tippt.</summary>
    public double HomeWinBaselineRate => Games == 0 ? 0 : (double)Predictions.Count(p => p.ActualOutcome == MatchOutcome.HomeWin) / Games;
}
