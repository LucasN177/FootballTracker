using FootballTracker.Core.Models.Statistics;

namespace FootballTracker.Tests;

public class MatchPredictionTests
{
    [Fact]
    public void Balanced_game_gets_draw_tip_only_with_draw_strategy()
    {
        var matrix = new double[11, 11];
        matrix[1, 0] = 0.36;
        matrix[1, 1] = 0.30;
        matrix[0, 1] = 0.34;
        var prediction = TestData.Prediction(TestData.Open(1, 2), matrix, tip: (1, 0));

        Assert.True(prediction.IsBalanced);
        Assert.Equal((1, 0), prediction.GetTip(TipStrategy.KicktippOptimal));
        Assert.Equal((1, 1), prediction.GetTip(TipStrategy.WithDraws));
    }

    [Fact]
    public void Clear_favourite_keeps_winner_tip_with_draw_strategy()
    {
        var matrix = new double[11, 11];
        matrix[2, 0] = 0.6;
        matrix[1, 1] = 0.25;
        matrix[0, 1] = 0.15;
        var prediction = TestData.Prediction(TestData.Open(1, 2), matrix, tip: (2, 0));

        Assert.False(prediction.IsBalanced);
        Assert.Equal((2, 0), prediction.GetTip(TipStrategy.WithDraws));
    }

    [Fact]
    public void Expected_points_weight_kicktipp_points_by_probability()
    {
        var matrix = new double[11, 11];
        matrix[2, 1] = 0.5; // exakt: 4 Punkte
        matrix[1, 0] = 0.3; // Tordifferenz: 3 Punkte
        matrix[0, 0] = 0.2; // falsch: 0 Punkte
        var prediction = TestData.Prediction(TestData.Open(1, 2), matrix);

        Assert.Equal(0.5 * 4 + 0.3 * 3, prediction.GetExpectedPoints((2, 1)), precision: 10);
    }

    [Fact]
    public void Evaluation_uses_the_final_result()
    {
        var game = TestData.Finished(1, 2, 2, 1);
        var prediction = TestData.Prediction(game, new double[11, 11], tip: (2, 1));

        Assert.Equal((2, 1), prediction.ActualScore);
        Assert.True(prediction.IsExactHit(TipStrategy.KicktippOptimal));
        Assert.Equal(4, prediction.GetKicktippPoints(TipStrategy.KicktippOptimal));
    }

    [Fact]
    public void Open_game_has_no_evaluation()
    {
        var prediction = TestData.Prediction(TestData.Open(1, 2), new double[11, 11]);

        Assert.Null(prediction.ActualScore);
        Assert.Null(prediction.GetKicktippPoints(TipStrategy.KicktippOptimal));
        Assert.Null(prediction.BrierScore);
    }
}
