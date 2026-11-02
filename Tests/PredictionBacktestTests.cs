using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Tests;

public class PredictionBacktestTests
{
    /// <summary>Drei Spiele: Heimsieg exakt getippt, Remis im ausgeglichenen Spiel verpasst, Auswärtssieg falsch getippt.</summary>
    private static PredictionBacktest Create(TipStrategy strategy)
    {
        var balanced = new double[11, 11];
        balanced[1, 0] = 0.36;
        balanced[1, 1] = 0.30;
        balanced[0, 1] = 0.34;
        var favourite = new double[11, 11];
        favourite[2, 0] = 0.7;
        favourite[0, 1] = 0.3;

        return new PredictionBacktest
        {
            Strategy = strategy,
            Predictions =
            [
                TestData.Prediction(TestData.Finished(1, 2, 2, 0, 1), favourite, tip: (2, 0)),
                TestData.Prediction(TestData.Finished(3, 4, 1, 1, 1), balanced, tip: (1, 0)),
                TestData.Prediction(TestData.Finished(5, 6, 0, 2, 2), favourite, tip: (2, 0))
            ]
        };
    }

    [Fact]
    public void Kicktipp_optimal_counts_hits_and_points()
    {
        var backtest = Create(TipStrategy.KicktippOptimal);

        Assert.Equal(3, backtest.Games);
        Assert.Equal(1, backtest.TendencyHits);
        Assert.Equal(1, backtest.ExactHits);
        Assert.Equal(4, backtest.KicktippPoints);
        Assert.Equal(1, backtest.WinnerHits);
        Assert.Equal(2, backtest.DecidedGames);
        Assert.Equal(0, backtest.DrawHits);
        Assert.Equal(1, backtest.DrawGames);
        Assert.Equal(1.0 / 3, backtest.HomeWinBaselineRate, precision: 10);
    }

    [Fact]
    public void Draw_strategy_catches_the_balanced_draw()
    {
        var backtest = Create(TipStrategy.KicktippOptimal).WithStrategy(TipStrategy.WithDraws);

        Assert.Equal(2, backtest.TendencyHits);
        Assert.Equal(2, backtest.ExactHits);
        Assert.Equal(8, backtest.KicktippPoints);
        Assert.Equal(1, backtest.DrawHits);
    }

    [Fact]
    public void Brier_score_is_zero_for_certain_correct_forecasts()
    {
        var game = TestData.Finished(1, 2, 1, 0);
        var backtest = new PredictionBacktest { Predictions = [TestData.CertainResult(game, 1, 0)] };

        Assert.Equal(0, backtest.BrierScore, precision: 10);
    }

    [Fact]
    public void Groups_results_by_match_day()
    {
        var byMatchDay = Create(TipStrategy.KicktippOptimal).ByMatchDay();

        Assert.Equal([1, 2], byMatchDay.Select(d => d.MatchDay));
        Assert.Equal(2, byMatchDay[0].Games);
        Assert.Equal(1, byMatchDay[0].TendencyHits);
        Assert.Equal(0, byMatchDay[1].TendencyHits);
    }
}
