using FootballTracker.Core.Models.Statistics;
using FootballTracker.Services;

namespace FootballTracker.Tests;

public class PredictionModelTests
{
    private static readonly double[] Attack = [0.5, 0.3, 0.1, 0.0, 0.0, -0.1, -0.2, -0.3];
    private static readonly double[] Defense = [-0.4, -0.2, -0.1, 0.0, 0.1, 0.1, 0.2, 0.3];
    private const double HomeAdvantage = 0.25;

    private static List<PredictionModel.Sample> Samples(int rounds, int seed) =>
        TestData.SimulateLeague(Attack, Defense, HomeAdvantage, baseGoals: 1.2, rounds, seed)
            .Select(g => new PredictionModel.Sample(g.Team1.Id, g.Team2.Id, g.FinalResult!.PointsTeam1, g.FinalResult.PointsTeam2, 1))
            .ToList();

    private static PredictionModel FitSyntheticLeague() =>
        PredictionModel.Fit(Samples(rounds: 6, seed: 7), Enumerable.Range(1, Attack.Length), new HashSet<int>());

    [Fact]
    public void Recovers_home_advantage_and_strength_order()
    {
        var model = FitSyntheticLeague();

        Assert.InRange(model.HomeAdvantage, HomeAdvantage - 0.12, HomeAdvantage + 0.12);
        Assert.Equal(1, model.Ratings.MaxBy(r => r.Value.Attack - r.Value.Defense).Key);
        Assert.Equal(Attack.Length, model.Ratings.MinBy(r => r.Value.Attack - r.Value.Defense).Key);
    }

    [Fact]
    public void Probabilities_form_a_distribution()
    {
        var prediction = FitSyntheticLeague().Predict(TestData.Open(1, 8));

        double sum = 0;
        foreach (var p in prediction.ScoreProbabilities)
        {
            Assert.True(p >= 0);
            sum += p;
        }
        Assert.Equal(1, sum, precision: 9);
        Assert.Equal(1, prediction.HomeWin + prediction.Draw + prediction.AwayWin, precision: 9);
        Assert.Equal(prediction.Draw, Enumerable.Range(0, 11).Sum(k => prediction.ScoreProbabilities[k, k]), precision: 9);
    }

    [Fact]
    public void Tip_maximises_expected_kicktipp_points()
    {
        var prediction = FitSyntheticLeague().Predict(TestData.Open(1, 8));

        Assert.True(prediction.HomeWin > 0.6);
        Assert.Equal(MatchOutcome.HomeWin, MatchPrediction.GetOutcome(prediction.Tip));
        var best = prediction.GetExpectedPoints(prediction.Tip);
        for (var home = 0; home <= 5; home++)
            for (var away = 0; away <= 5; away++)
                Assert.True(prediction.GetExpectedPoints((home, away)) <= best + 1e-12);
    }

    [Fact]
    public void Draw_tip_is_the_most_likely_draw()
    {
        var prediction = FitSyntheticLeague().Predict(TestData.Open(4, 5));

        var mostLikelyDraw = Enumerable.Range(0, 11).MaxBy(k => prediction.ScoreProbabilities[k, k]);
        Assert.Equal((mostLikelyDraw, mostLikelyDraw), prediction.DrawTip);
    }

    [Fact]
    public void New_teams_start_below_league_average()
    {
        const int newTeam = 99;
        var model = PredictionModel.Fit(Samples(rounds: 2, seed: 3), Enumerable.Range(1, Attack.Length).Append(newTeam), new HashSet<int> { newTeam });

        Assert.True(model.Ratings[newTeam].Attack < 0);
        Assert.True(model.Ratings[newTeam].Defense > 0);
        Assert.True(model.Predict(TestData.Open(4, newTeam)).HomeWin > model.Predict(TestData.Open(newTeam, 4)).AwayWin);
    }

    [Fact]
    public void Expected_goals_are_the_product_of_the_factors()
    {
        var model = FitSyntheticLeague();
        var prediction = model.Predict(TestData.Open(2, 6));
        var home = model.GetRating(2);
        var away = model.GetRating(6);

        Assert.Equal(model.BaseGoals * model.HomeFactor * Math.Exp(home.Attack) * Math.Exp(away.Defense), prediction.ExpectedGoalsHome, precision: 10);
        Assert.Equal(model.BaseGoals * Math.Exp(away.Attack) * Math.Exp(home.Defense), prediction.ExpectedGoalsAway, precision: 10);
    }

    [Fact]
    public void Unknown_team_gets_league_average()
    {
        var model = FitSyntheticLeague();

        Assert.Equal(new PredictionModel.Rating(0, 0), model.GetRating(12345));
        Assert.False(model.IsNewTeam(12345));
    }

    [Fact]
    public void Dixon_coles_only_changes_low_scores()
    {
        Assert.Equal(1, PredictionModel.Tau(2, 1, 1.5, 1.2, -0.1));
        Assert.Equal(1.1, PredictionModel.Tau(1, 1, 1.5, 1.2, -0.1), precision: 10);
        Assert.Equal(1 + 1.5 * 1.2 * 0.1, PredictionModel.Tau(0, 0, 1.5, 1.2, -0.1), precision: 10);
        Assert.Equal(1, PredictionModel.PoissonPmf(1.7).Sum(), precision: 4);
    }

    [Fact]
    public void Weight_halves_after_half_life()
    {
        var reference = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        Assert.Equal(1, PredictionModel.GetWeight(reference, reference), precision: 10);
        Assert.Equal(0.5, PredictionModel.GetWeight(reference.AddDays(-PredictionModel.HalfLifeDays), reference), precision: 10);
        Assert.Equal(1, PredictionModel.GetWeight(reference.AddDays(1), reference), precision: 10);
    }

    [Fact]
    public void Rho_stays_in_the_searched_range()
    {
        Assert.InRange(FitSyntheticLeague().Rho, -0.25, 0.15);
    }
}
