using FootballTracker.Core.Models;
using FootballTracker.Core.Models.Extensions;
using FootballTracker.Services;

namespace FootballTracker.Tests;

public class StatisticsServiceTests
{
    [Fact]
    public void Records_are_sorted_by_points_goal_difference_and_goals()
    {
        List<Game> games =
        [
            TestData.Finished(1, 2, 1, 0),
            TestData.Finished(3, 4, 3, 1),
            TestData.Finished(2, 4, 2, 2)
        ];

        var records = StatisticsService.BuildRecords(games);

        Assert.Equal([3, 1, 2, 4], records.Select(r => r.Team.Id));
        Assert.Equal(1, records.Single(r => r.Team.Id == 2).Points);
        Assert.Equal(1, records.Single(r => r.Team.Id == 1).CleanSheets);
    }

    [Fact]
    public void Penalty_shootout_decides_the_outcome()
    {
        var game = TestData.Finished(1, 2, 1, 1);
        game.MatchResults.Add(new MatchResult { ResultTypeId = 5, PointsTeam1 = 5, PointsTeam2 = 4 });

        Assert.Equal('S', game.GetOutcomeFor(1));
        Assert.Equal('N', game.GetOutcomeFor(2));
        Assert.Equal((1, 1), game.GetGoalsFor(1));
    }
}
