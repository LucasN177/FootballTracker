using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Statistics;
using FootballTracker.Services;

namespace FootballTracker.Tests;

public class SeasonSimulatorTests
{
    /// <summary>Jedes Ergebnis von 0:0 bis 3:3 gleich wahrscheinlich – alle Teams gleich stark.</summary>
    private static MatchPrediction Uniform(Game game)
    {
        var matrix = new double[11, 11];
        for (var i = 0; i <= 3; i++)
            for (var j = 0; j <= 3; j++)
                matrix[i, j] = 1.0 / 16;
        return TestData.Prediction(game, matrix);
    }

    [Fact]
    public void Finished_season_has_a_fixed_table()
    {
        List<Game> games =
        [
            TestData.Finished(1, 2, 2, 0),
            TestData.Finished(2, 3, 1, 1),
            TestData.Finished(3, 1, 0, 1)
        ];
        var simulator = new SeasonSimulator(games, Uniform, seed: 1);
        simulator.Run(200);
        var result = simulator.GetResult();

        Assert.Equal(0, result.RemainingGames);
        // Team 2 und 3 haben je einen Punkt, Team 3 die bessere Tordifferenz (−1 gegen −2)
        Assert.Equal([1, 3, 2], result.Teams.Select(t => t.Team.Id));
        Assert.Equal(1, result.Teams[0].PositionProbabilities[0]);
        Assert.Equal(6, result.Teams[0].ExpectedPoints);
    }

    [Fact]
    public void Ties_are_broken_by_goal_difference_then_goals_then_lot()
    {
        List<Game> games =
        [
            TestData.Finished(1, 3, 1, 0), // Team 1: 3 Pkt, +1
            TestData.Finished(2, 4, 3, 0), // Team 2: 3 Pkt, +3
            TestData.Finished(5, 6, 2, 2),
            TestData.Finished(6, 5, 1, 1)  // Team 5 und 6: je 2 Pkt, ±0, 3 Tore
        ];
        var simulator = new SeasonSimulator(games, Uniform, seed: 1);
        simulator.Run(2000);
        var teams = simulator.GetResult().Teams.ToDictionary(t => t.Team.Id);

        Assert.Equal(1, teams[2].PositionProbabilities[0]);
        Assert.Equal(1, teams[1].PositionProbabilities[1]);
        Assert.InRange(teams[5].PositionProbabilities[2], 0.45, 0.55);
        Assert.Equal(1, teams[5].GetProbability(3, 4), precision: 10);
    }

    [Fact]
    public void Certain_open_result_decides_the_title()
    {
        List<Game> games =
        [
            TestData.Finished(1, 2, 1, 1),
            TestData.Open(2, 1, matchDay: 2)
        ];
        var simulator = new SeasonSimulator(games, g => TestData.CertainResult(g, 0, 2), seed: 1);
        simulator.Run(500);
        var result = simulator.GetResult();

        Assert.Equal(1, result.RemainingGames);
        var team1 = result.Teams.Single(t => t.Team.Id == 1);
        Assert.Equal(1, team1.PositionProbabilities[0]);
        Assert.Equal(4, team1.ExpectedPoints);
        Assert.Equal(1, team1.Points);
    }

    [Fact]
    public void Probabilities_sum_to_one_per_team_and_per_position()
    {
        var games = new List<Game>();
        var matchDay = 0;
        for (var home = 1; home <= 4; home++)
            for (var away = 1; away <= 4; away++)
                if (home != away)
                    games.Add(TestData.Open(home, away, ++matchDay));
        var simulator = new SeasonSimulator(games, Uniform, seed: 42);
        simulator.Run(3000);
        var result = simulator.GetResult();

        Assert.Equal(3000, result.Runs);
        foreach (var team in result.Teams)
            Assert.Equal(1, team.PositionProbabilities.Sum(), precision: 10);
        for (var position = 0; position < 4; position++)
            Assert.Equal(1, result.Teams.Sum(t => t.PositionProbabilities[position]), precision: 10);
        Assert.All(result.Teams, t => Assert.InRange(t.ExpectedPosition, 2.3, 2.7));
    }

    [Fact]
    public void Same_seed_gives_same_result()
    {
        List<Game> games = [TestData.Open(1, 2), TestData.Open(2, 1, 2), TestData.Open(1, 3, 3)];
        var first = new SeasonSimulator(games, Uniform, seed: 5);
        var second = new SeasonSimulator(games, Uniform, seed: 5);
        first.Run(1000);
        second.Run(1000);

        Assert.Equal(
            first.GetResult().Teams.Select(t => t.ExpectedPoints),
            second.GetResult().Teams.Select(t => t.ExpectedPoints));
    }
}
